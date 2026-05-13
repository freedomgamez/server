using DigitalWorldOnline.Application;
using DigitalWorldOnline.Application.GameAssets;
using DigitalWorldOnline.Application.GameAssets.Bins;
using DigitalWorldOnline.Application.Separar.Commands.Update;
using DigitalWorldOnline.Commons.Entities;
using DigitalWorldOnline.Commons.Enums;
using DigitalWorldOnline.Commons.Enums.ClientEnums;
using DigitalWorldOnline.Commons.Enums.PacketProcessor;
using DigitalWorldOnline.Commons.Interfaces;
using DigitalWorldOnline.Commons.Models.Digimon;
using DigitalWorldOnline.Commons.Packets.Chat;
using DigitalWorldOnline.Commons.Packets.GameServer;
using DigitalWorldOnline.Commons.Packets.GameServer.Combat;
using DigitalWorldOnline.Commons.Packets.Items;
using DigitalWorldOnline.Commons.Utils;
using DigitalWorldOnline.GameHost;
using MediatR;
using Serilog;

namespace DigitalWorldOnline.Game.PacketProcessors
{
    /// <summary>
    /// Handles <c>pSkill::UseSkillChip</c> (1120) — client requests a cast of a memory
    /// skill currently attached to the partner's active evolution.  v487 wire payload
    /// (cCliGame::SendUseDigimonMemorySkill, cCliGameSend.cpp:2373):
    ///   u4 nDigimonUID, u1 nEvoStep, u4 nSkillCode, u4 nTargetUID
    ///
    /// Server validates ownership and cooldown, computes damage from the bin's CsSkill
    /// row (Apply[0].Value + level × IncreaseValue + partner stats), applies it to the
    /// target mob, then broadcasts:
    ///   - SkillHitPacket (1102) — damage application (re-used from the inherent-skill
    ///     path); client deducts HP and renders the hit numbers.
    ///   - MemorySkillEffectSyncPacket (1122) — cosmetic effect-sync that the v487
    ///     client uses to play heal/buff VFX off `s_Apply[0].s_nA`.
    ///
    /// Damage formula mirrors <c>PartnerSkillPacketProcessor.CalculateDamageOrHeal</c>
    /// but reads the level from <see cref="Commons.Models.Digimon.DigimonMemorySkillModel.CurrentLevel"/>
    /// instead of the inherent-slot table (memory skills don't occupy slot 0..3).
    /// </summary>
    public class MemorySkillUsePacketProcessor : IGamePacketProcessor
    {
        public GameServerPacketEnum Type => GameServerPacketEnum.MemorySkillUse;

        private const int CTypeClassIdxMask = 0x7FFFF;

        private readonly ILogger _logger;
        private readonly ISender _sender;
        private readonly AssetsLoader _assets;
        private readonly BuffBinLoader _buffBin;
        private readonly MapServer _mapServer;
        private readonly DungeonsServer _dungeonServer;

        public MemorySkillUsePacketProcessor(
            ILogger logger, ISender sender, AssetsLoader assets,
            BuffBinLoader buffBin,
            MapServer mapServer, DungeonsServer dungeonServer)
        {
            _logger = logger;
            _sender = sender;
            _assets = assets;
            _buffBin = buffBin;
            _mapServer = mapServer;
            _dungeonServer = dungeonServer;
        }

        public async Task Process(GameClient client, byte[] packetData)
        {
            var packet = new GamePacketReader(packetData);

            int digimonUID = packet.ReadInt();
            byte evoStep = packet.ReadByte();
            int skillCode = packet.ReadInt();
            int targetUID = packet.ReadInt();

            // Strip the type bits the client packs into cType — server handlers store
            // class+idx only.  Same mask as PartnerSkillPacketProcessor.
            int attackerHandler = digimonUID & CTypeClassIdxMask;
            int targetHandler = targetUID & CTypeClassIdxMask;

            if (client.Partner == null)
            {
                _logger.Warning("MemorySkillUse: tamer {TamerId} has no Partner — dropping cast.", client.TamerId);
                return;
            }

            var evolution = client.Partner.Evolutions.FirstOrDefault(x => x.Type == client.Partner.CurrentType);
            if (evolution == null)
            {
                _logger.Warning(
                    "MemorySkillUse: tamer {TamerId} has no Evolution matching CurrentType={CurrentType} (available types: [{Avail}]).",
                    client.TamerId, client.Partner.CurrentType,
                    string.Join(",", client.Partner.Evolutions.Select(e => e.Type)));
                return;
            }

            var owned = evolution.MemorySkills.FirstOrDefault(x => x.SkillId == skillCode);
            if (owned == null)
            {
                _logger.Warning(
                    "Tamer {TamerId} tried to cast memory skill {SkillId} on evolution {EvoType} without owning it.",
                    client.TamerId, skillCode, evolution.Type);
                return;
            }

            if (owned.IsOnCooldown)
                return;

            var skillInfo = _assets.SkillInfo.FirstOrDefault(x => x.SkillId == skillCode);
            if (skillInfo == null || !skillInfo.IsMemorySkill || skillInfo.IsPassive)
            {
                _logger.Warning(
                    "Tamer {TamerId} requested memory-skill cast for {SkillId} but bin doesn't mark it as a castable memory skill.",
                    client.TamerId, skillCode);
                return;
            }

            var skillCodeInfo = _assets.SkillCodeInfo.FirstOrDefault(x => x.SkillCode == skillCode);
            if (skillCodeInfo == null)
            {
                _logger.Warning("Tamer {TamerId} memory-skill {SkillId} has no SkillCode row in bin.",
                    client.TamerId, skillCode);
                return;
            }

            // Memory skills split into damage vs buff/stat by the bin's APPLY FORMULA
            // (Apply[0].Id, exposed as Apply[0].Type in the asset model — maps to the
            // SkillCodeApplyTypeEnum):
            //   * Formula 1/2/10 = direct damage formulas (PA-... etc).  These reduce
            //     the targeted mob's HP.  Apply.B = base damage, Apply.A = which
            //     attribute is hit (1 = HP).  Example: Ruler of Space (9000043/44/45,
            //     target=17 AoE-around-partner).
            //   * Formula 101+, 200+ = stat-modify formulas (A = A + B, percent, timed,
            //     conditional, etc.).  These apply a buff to the partner.  Example:
            //     Ruler of Earth Mid (9000041, target=13, Apply.A=43 SkillDamageByAttribute,
            //     formula=200 "no immediate effect — buff entry").
            // Routing by bin's s_nTarget alone is wrong — target=17 has both damage AND
            // stat-buff variants depending on formula.
            var apply0Damage = skillCodeInfo.Apply.FirstOrDefault();
            int formula = apply0Damage != null ? (int)apply0Damage.Type : 0;
            bool isDamageMemorySkill = formula == 1 || formula == 2 || formula == 10;
            bool isSelfCast = !isDamageMemorySkill;
            Commons.Models.Config.MobConfigModel? targetMob = null;
            var mapId = client.Tamer.Location.MapId;

            if (!isSelfCast)
            {
                targetMob = client.DungeonMap
                    ? _dungeonServer.GetMobByHandler(mapId, targetHandler, client.TamerId)
                    : _mapServer.GetMobByHandler(mapId, targetHandler);

                if (targetMob == null)
                {
                    _logger.Warning(
                        "MemorySkillUse: tamer {TamerId} cast skill {SkillId} but no mob at handler 0x{Target:X8} on map {MapId} (dungeon={Dungeon}).",
                        client.TamerId, skillCode, targetHandler, mapId, client.DungeonMap);
                    return;
                }
                if (!targetMob.Alive) return;
            }

            // Resource costs come from the skill's bin row — memory skills aren't free.
            client.Partner.ReceiveDamage(skillInfo.HPUsage);
            client.Partner.UseDs(skillInfo.DSUsage);
            client.Partner.SetEndCasting(0);

            if (isSelfCast)
            {
                // Self-cast (buff / heal): no mob, no damage.  Effect kind is in
                // skillCodeInfo.Apply[0].Type (1=heal HP, 3=stat-up, 9=movespeed, 19=evasion,
                // 47=conditional heal — per RecvMemorySkill_EffectSync in cCliGameSkill.cpp).
                int partnerHandler = client.Partner.GeneralHandler;

                // 1) Consume memory chips from inventory.  Chip count per cast = bin's
                //    s_nReq_Item on this skill (SkillInfoAssetModel.MemoryChips).  v487
                //    client only checks chip count locally; the server is authoritative
                //    here so a manipulated client can't bypass the cost.  The chip items
                //    are item 20000 (Memory Chip) and 19999 (Memory Chip Event), both in
                //    section 7000 — RemoveOrReduceItemsBySection treats them as one pool.
                const int MemoryChipSection = 7000;
                int chipsNeeded = Math.Max(1, (int)skillInfo.MemoryChips);
                if (!client.Tamer.Inventory.RemoveOrReduceItemsBySection(MemoryChipSection, chipsNeeded))
                {
                    _logger.Warning(
                        "MemorySkillUse: tamer {TamerId} skill {SkillId} needs {Need} memory chips (section {Sec}) — insufficient, aborting.",
                        client.TamerId, skillCode, chipsNeeded, MemoryChipSection);
                    client.Send(new SystemMessagePacket($"Not enough memory chips."));
                    return;
                }
                await _sender.Send(new UpdateItemsCommand(client.Tamer.Inventory));
                client.Send(new LoadInventoryPacket(client.Tamer.Inventory, InventoryTypeEnum.Inventory).Serialize());

                // 2) Apply the actual effect.  Memory skills split into two shapes:
                //
                //    a) Instant-effect skills (Apply[0].Type == 1 = heal, also 47 conditional
                //       heal).  These have NO BuffInfo row in v487 — they execute once and
                //       finish.  Server heals the partner immediately and broadcasts an
                //       HP-rate update.  The client's RecvMemorySkill_EffectSync (1122) plays
                //       the Buff_Hp_Absorb.nif VFX for Apply.Type == 1 or 47, so we don't
                //       need an extra buff packet for the visual.
                //
                //    b) Buff-on-partner skills (everything else — conditional damage/defense
                //       boosts like Apply.Type == 42/43, persistent stat-ups).  These DO have
                //       a BuffInfo row in Buff.bin (usually flagged s_bDelete=true; we load
                //       it anyway from the raw bin loader).  Server adds the buff to
                //       Partner.BuffList, persists, broadcasts AddBuffPacket so the icon
                //       appears with a real timer.
                // Effect kind comes from the bin's s_Apply[0].s_nA, which the asset
                // mapper exposes as `Attribute` (NOT `Type` — `Type` maps to s_dwBuffID).
                // Per cCliGameSkill.cpp's RecvMemorySkill_EffectSync switch on s_nA:
                //   1  = HP heal
                //   3  = max HP/DS up
                //   9  = move speed up
                //   19 = evasion up
                //   42 = HP-conditional damage bonus
                //   43 = HP-conditional defense bonus
                //   47 = conditional heal
                var apply0 = skillCodeInfo.Apply.FirstOrDefault();
                int applyAttr = apply0 != null ? (int)apply0.Attribute : 0;
                bool isInstantHeal = applyAttr == 1 || applyAttr == 47;

                if (isInstantHeal)
                {
                    // Resolve heal amount by FORMULA, not just Apply.B raw value.  The
                    // bin's Apply.Id (mapped to SkillCodeApplyTypeEnum) tells you how to
                    // interpret B:
                    //   101 Default      → A = A + B               (flat HP)
                    //   102 Percent      → BA = BA + (BA×B÷100)    (% of max attribute)
                    //   105 Unknown105   → A = A + (A×B÷100)       (% of current attribute)
                    //   106 AlsoPercent  → +B%                     (% of max — heal items)
                    //   200 Unknown200   → no immediate effect      (buff entry only)
                    // Without this routing, "The Hand of Healing Low" (9000601: Apply.A=1,
                    // formula=106, B=15) heals a flat 15 HP instead of 15% of MaxHP.
                    int applyFormula = apply0 != null ? (int)apply0.Type : 0;
                    int basePoint = (apply0?.Value ?? 0)
                                  + ((owned.CurrentLevel - 1) * (apply0?.IncreaseValue ?? 0));

                    int healAmount;
                    switch (applyFormula)
                    {
                        case 102:   // % of max
                        case 106:   // % of max (DMO heal-item convention)
                            healAmount = (int)Math.Round(client.Partner.HP * (basePoint / 100.0));
                            break;
                        case 105:   // % of current
                            healAmount = (int)Math.Round(client.Partner.CurrentHp * (basePoint / 100.0));
                            break;
                        case 101:   // flat
                        default:
                            healAmount = basePoint;
                            break;
                    }

                    if (healAmount <= 0) healAmount = 1;
                    client.Partner.RecoverHp(healAmount);

                    // HP-rate broadcast: pGame::HpRate (1007) has an `assert_cs(pObject->GetLeafRTTI() != RTTI_DIGIMON_USER)`
                    // in cCliGameReceive.cpp:2083 — the v487 client refuses HpRate updates for
                    // the local player's own partner.  Send it to other tamers in view via
                    // BroadcastForTargetTamers (which excludes self), and update the caster's
                    // own UI with UpdateStatusPacket which carries full HP/DS values.
                    var hpRatePkt = new UpdateCurrentHPRatePacket(partnerHandler, client.Partner.HpRate).Serialize();
                    if (client.DungeonMap)
                        _dungeonServer.BroadcastForTargetTamers(client.TamerId, hpRatePkt);
                    else
                        _mapServer.BroadcastForTargetTamers(client.TamerId, hpRatePkt);

                    client.Send(new UpdateStatusPacket(client.Tamer));

                    // Heal VFX is driven by the effect-sync (1122) which the client maps
                    // to "Buff_Hp_Absorb.nif" on Apply.Type 1/47.  Send it now and we're
                    // done — no buff to add, instant effects don't persist.
                    var healSync = new MemorySkillEffectSyncPacket(partnerHandler, skillCode).Serialize();
                    if (client.DungeonMap)
                        _dungeonServer.BroadcastForTamerViewsAndSelf(client.TamerId, healSync);
                    else
                        _mapServer.BroadcastForTamerViewsAndSelf(client.TamerId, healSync);
                }
                else
                {

                // Memory-skill buffs in v487 are flagged s_bDelete=true in Buff.bin
                // (regional-disable convention from original DMO).  AssetsLoader's
                // BuffInfo list filters those out, so we query the raw bin loader
                // here, tolerating deleted entries.  Use the int-args overload of
                // AddBuffPacket so we don't need to reconstruct a populated
                // BuffInfoAssetModel for these deleted rows.
                var rawBuff = _buffBin.Data.ById.Values.FirstOrDefault(b =>
                    b.SkillCode == (uint)skillCode || b.DigimonSkillCode == (uint)skillCode);

                if (rawBuff != null)
                {
                    // Memory skills don't carry an explicit duration in the bin; use the
                    // DMO convention of 30 minutes (1800 s).  Stack policy: if an instance
                    // with the same skill is already active, remove + re-add to refresh.
                    const int MemorySkillBuffDurationSeconds = 1800;

                    var existing = client.Partner.BuffList.ActiveBuffs.FirstOrDefault(x => x.SkillId == skillCode);
                    if (existing != null)
                    {
                        client.Partner.BuffList.Buffs.Remove(existing);
                        if (client.DungeonMap)
                            _dungeonServer.BroadcastForTamerViewsAndSelf(client.TamerId,
                                new RemoveBuffPacket(partnerHandler, existing.BuffId).Serialize());
                        else
                            _mapServer.BroadcastForTamerViewsAndSelf(client.TamerId,
                                new RemoveBuffPacket(partnerHandler, existing.BuffId).Serialize());
                    }

                    var newBuff = DigimonBuffModel.Create(
                        buffId:   (int)rawBuff.Id,
                        skillId:  skillCode,
                        TypeN:    0,
                        duration: MemorySkillBuffDurationSeconds,
                        Cooldown: 0);
                    client.Partner.BuffList.Add(newBuff);

                    // Persist to DB so the buff survives relog (matches the pattern in
                    // TamerSkillRequestPacketProcessor — every BuffList mutation that
                    // should outlive the session is followed by an UpdateDigimonBuffList).
                    await _sender.Send(new UpdateDigimonBuffListCommand(client.Partner.BuffList));

                    // Wire field is named s_nBuffEndTS but the v487 client subtracts
                    // _TIME_TS from it to get "remaining seconds" — and _TIME_TS is
                    // hard-zero in this build (cClient::m_timets is initialized to 0
                    // in the constructor and there is no setter anywhere in the leaked
                    // nlib).  So we send the raw duration in seconds, NOT a Unix-epoch
                    // timestamp.  UtilitiesFunctions.RemainingTimeSeconds(N) would have
                    // returned ~1.747e9 (Unix epoch+N), which the client then displayed
                    // as ~20,231 days.  Sending N directly displays as N seconds → "30 M".
                    var dur = MemorySkillBuffDurationSeconds;
                    var addBuffPacket = new AddBuffPacket(partnerHandler, (int)rawBuff.Id, skillCode, (short)0, dur).Serialize();
                    if (client.DungeonMap)
                        _dungeonServer.BroadcastForTamerViewsAndSelf(client.TamerId, addBuffPacket);
                    else
                        _mapServer.BroadcastForTamerViewsAndSelf(client.TamerId, addBuffPacket);
                }
                else
                {
                    _logger.Warning(
                        "MemorySkillUse: tamer {TamerId} skill {SkillId} has no Buff.bin record at all — effect-sync sent but no buff applied.",
                        client.TamerId, skillCode);
                }

                // 3) Effect-sync (1122) for the buff branch — drives on-partner VFX
                //    (instant-heal branch above already broadcast its own copy).
                var buffSync = new MemorySkillEffectSyncPacket(partnerHandler, skillCode).Serialize();
                if (client.DungeonMap)
                    _dungeonServer.BroadcastForTamerViewsAndSelf(client.TamerId, buffSync);
                else
                    _mapServer.BroadcastForTamerViewsAndSelf(client.TamerId, buffSync);
                } // end of else (non-heal)
            }
            else
            {
                // Enemy-target damage path — targetMob is guaranteed non-null + alive here.
                // (mob-target memory skills aren't observed for any 9000xxx bin entry today,
                // but the path is wired for completeness in case a future bin adds one.)
                int finalDmg;
                if (client.Tamer.GodMode)
                {
                    finalDmg = targetMob!.CurrentHP;
                }
                else
                {
                    var apply = skillCodeInfo.Apply.FirstOrDefault(x => x.Type > 0);
                    double baseDmg = apply != null
                        ? apply.Value + (owned.CurrentLevel * apply.IncreaseValue)
                        : 0;

                    var skillFactor = (double)((decimal)client.Partner.SCD / 100);
                    double cloneFactor = 1.0;
                    if (client.Tamer.Partner.Digiclone?.ATValue > 0)
                    {
                        double factorFromPF = 144.0 / client.Tamer.Partner.Digiclone.ATValue;
                        cloneFactor = Math.Round(1.0 + (0.43 / factorFromPF), 2);
                    }
                    baseDmg = Math.Floor(baseDmg * cloneFactor);
                    double addedDmg = Math.Floor(baseDmg * skillFactor / 100.0);

                    finalDmg = (int)Math.Floor(baseDmg + addedDmg + client.Tamer.Partner.AT + client.Tamer.Partner.SKD);
                    finalDmg = ApplySkillDamagePercentBonus(finalDmg, client.Tamer.Partner.SkillDamagePercent);
                    if (finalDmg <= 0) finalDmg = 1;
                }

                if (finalDmg > targetMob!.CurrentHP) finalDmg = targetMob.CurrentHP;

                if (!client.Tamer.InBattle)
                {
                    client.Tamer.SetHidden(false);
                    _mapServer.BroadcastForTamerViewsAndSelf(client.TamerId, new SetCombatOnPacket(attackerHandler).Serialize());
                    client.Tamer.StartBattleWithSkill(new List<Commons.Models.Config.MobConfigModel> { targetMob }, SkillTypeEnum.Single);
                }
                else
                {
                    client.Tamer.SetHidden(false);
                    client.Tamer.UpdateTargetWithSkill(new List<Commons.Models.Config.MobConfigModel> { targetMob }, SkillTypeEnum.Single);
                }

                if (!targetMob.InBattle)
                {
                    _mapServer.BroadcastForTamerViewsAndSelf(client.TamerId, new SetCombatOnPacket(targetHandler).Serialize());
                    targetMob.StartBattle(client.Tamer);
                }
                else
                {
                    targetMob.AddTarget(client.Tamer);
                }

                var newHp = targetMob.ReceiveDamage(finalDmg, client.TamerId);

                if (newHp <= 0)
                {
                    // Memory-skill kill broadcast.  Order matters:
                    //   1) SyncConditionPacket(handler, Die) — drives the v487 client's
                    //      cCondition::SetCondition → SetDie() → MONSTER_DIE state
                    //      transition, which is what actually plays the death animation
                    //      and despawns the mob.  Partner-skill's KillOnSkillPacket
                    //      alone works because its skill-animation chain queues a
                    //      DT_Dead entry that AttackProperty eventually applies via
                    //      SetDie(); the memory-skill cast in v487 (UseSkillChip 1120)
                    //      bypasses that animation pipeline, so the AttackProperty
                    //      loop never fires SetDie on its own.  An explicit Sync(Die)
                    //      forces the state transition.  Must come BEFORE KillOnSkill
                    //      because cCondition::SetCondition early-returns once
                    //      IsServerDie()==true (Condition.cpp:34-35).
                    //   2) KillOnSkillPacket — drives the damage-number popup and
                    //      flips m_bServerDie via SetDieInfo.
                    _mapServer.BroadcastForTamerViewsAndSelf(
                        client.TamerId,
                        new SyncConditionPacket(targetMob.GeneralHandler, ConditionEnum.Die).Serialize());

                    _mapServer.BroadcastForTamerViewsAndSelf(
                        client.TamerId,
                        new KillOnSkillPacket(
                            attackerHandler,
                            targetMob.GeneralHandler,
                            0,
                            finalDmg).Serialize());

                    _logger.Verbose("Tamer {TamerId} killed mob {MobId} ({MobName}) with memory skill {SkillId} for {Dmg}.",
                        client.TamerId, targetMob.Id, targetMob.Name, skillCode, finalDmg);
                    targetMob.Die();

                    // Mirror PartnerSkillPacketProcessor's post-kill cleanup: if the
                    // tamer no longer has any mobs targeting them, drop battle state
                    // and broadcast BattleOff.  Without this the partner stays in
                    // combat-stance after a one-shot memory-skill kill.
                    if (!_mapServer.MobsAttacking(client.Tamer.Location.MapId, client.TamerId))
                    {
                        client.Tamer.StopBattle();
                        _mapServer.BroadcastForTamerViewsAndSelf(
                            client.TamerId,
                            new SetCombatOffPacket(attackerHandler).Serialize());
                    }
                }
                else
                {
                    _mapServer.BroadcastForTamerViewsAndSelf(
                        client.TamerId,
                        new SkillHitPacket(
                            attackerHandler,
                            targetMob.GeneralHandler,
                            0,
                            finalDmg,
                            targetMob.CurrentHpRate).Serialize());

                    // Effect-sync (1122) — VFX on the surviving mob only.  Partner-skill
                    // doesn't send this; it's a memory-skill-specific cosmetic packet.
                    _mapServer.BroadcastForTamerViewsAndSelf(
                        client.TamerId,
                        new MemorySkillEffectSyncPacket(targetMob.GeneralHandler, skillCode).Serialize());

                    _logger.Verbose("Tamer {TamerId} hit mob {MobId} for {Dmg} with memory skill {SkillId}.",
                        client.TamerId, targetMob.Id, finalDmg, skillCode);
                }
            }

            static int ApplySkillDamagePercentBonus(int baseDamage, int percentPoints)
            {
                if (baseDamage <= 0 || percentPoints == 0)
                    return baseDamage;

                long scaled = (long)baseDamage * (100L + percentPoints);
                long adjusted = scaled / 100L;

                if (adjusted > int.MaxValue) return int.MaxValue;
                if (adjusted < int.MinValue) return int.MinValue;
                return (int)adjusted;
            }

            // Cooldown: `SkillInfoAssetModel.Cooldown` is ALREADY in milliseconds —
            // SkillInfoAssetsQueryHandler does `Cooldown = (int)rec.CooldownTimeMs`, where
            // CooldownTimeMs is loaded as the float s_fCooldownTime from the bin in ms
            // (e.g., 3000 = 3 s for skill 9000041).  An earlier version of this line
            // multiplied by 1000 again, producing a 50-minute lockout per cast.
            var cooldownMs = Math.Max(0, skillInfo.Cooldown);
            owned.StartCooldown(cooldownMs);
            await _sender.Send(new UpdateMemorySkillCooldownCommand(evolution.Id, skillCode, owned.CooldownEndsAt));

            // pSkill::ChipCoolTime (1121) — sent ONLY to the caster.  This packet starts
            // the cooldown swirl on the skill icon and decrements the memory-chip stack
            // visually on the client.  Without it the cast goes through but the icon
            // never enters cooldown state.  See RecvDigimonMemorySkill_UseSuccess in
            // cCliGameSkill.cpp.
            client.Send(new MemorySkillUseSuccessPacket(skillCode, cooldownMs).Serialize());
        }
    }
}
