using DigitalWorldOnline.Commons.Enums.ClientEnums;
using DigitalWorldOnline.Commons.Models.Asset;
using DigitalWorldOnline.Commons.Models.Assets;
using DigitalWorldOnline.Commons.Models.Character;
using DigitalWorldOnline.Commons.Models.Config;
using DigitalWorldOnline.Commons.Models.Digimon;
using DigitalWorldOnline.Commons.Models.Summon;
using DigitalWorldOnline.Commons.Packets.GameServer;
using DigitalWorldOnline.Commons.Packets.GameServer.Arena;
using DigitalWorldOnline.Commons.Packets.GameServer.Combat;
using DigitalWorldOnline.Commons.Packets.MapServer;
using DigitalWorldOnline.Commons.Utils;
using System.Diagnostics;

namespace DigitalWorldOnline.Commons.Models.Map
{
    public sealed partial class GameMap
    {
        private List<MobConfigModel> _mobsToDestroy = new List<MobConfigModel>();

        private List<MobConfigModel> _mobsToAdd = new List<MobConfigModel>();

        public List<long> NearestTamers(long mobId)
        {
            return ConnectedTamers.Where(x => x.MobsInView.Contains(mobId)).Select(x => x.Id).ToList();
        }

        public List<long> FarawayTamers(MobConfigModel mob)
        {
            var targetTamers = new List<long>();

            foreach (var tamer in ConnectedTamers)
            {
                var diff = UtilitiesFunctions.CalculateDistance(
                    mob.CurrentLocation.X,
                    tamer.Partner.Location.X,
                    mob.CurrentLocation.Y,
                    tamer.Partner.Location.Y);

                if (diff >= _stopSeeing)
                    targetTamers.Add(tamer.Id);
            }

            return targetTamers;
        }

        public void UpdateMapMobs()
        {
            if (!UpdateMobs)
                return;

            var mobsToRemove = new List<MobConfigModel>();

            foreach (var mob in Mobs)
                CheckRemoveMob(mobsToRemove, mob);

            foreach (var mob in mobsToRemove)
                RemoveMob(mob);

            //foreach (var mob in _mobsToAdd)
            //   AddMob(mob);

            FinishMobsUpdate();
        }
        public void UpdateMapMobs(List<NpcColiseumAssetModel> npcAsset)
        {
            

            var mobsToRemove = new List<MobConfigModel>();

            foreach (var mob in Mobs)
                CheckRemoveMob(mobsToRemove, mob, npcAsset);

            foreach (var mob in mobsToRemove)
                RemoveMob(mob);

            //foreach (var mob in _mobsToAdd)
            //   AddMob(mob);

            FinishMobsUpdate();
        }

        public void UpdateMapMobs(bool summon)
        {


            var mobsToRemove = new List<SummonMobModel>();

            foreach (var mob in SummonMobs)
                CheckRemoveMob(mobsToRemove, mob);

            foreach (var mob in mobsToRemove)
                RemoveMob(mob);

            //foreach (var mob in _mobsToAdd)
            //    AddMob(mob);
        }


        private void CheckRemoveMob(List<MobConfigModel> mobsToRemove, MobConfigModel mob)
        {
            if (_mobsToDestroy.Contains(mob))
            {
                foreach (var view in mob.TamersViewing)
                {
                    var targetClient = Clients.FirstOrDefault(x => x.TamerId == view);

                    if (targetClient != null)
                    {
                        targetClient.Tamer.RemoveTarget(mob);

                        targetClient.Send(new DestroyMobsPacket(mob));
                    }
                }

                mob.Destroy();

                mobsToRemove.Add(mob);
            }


        }
        private void CheckRemoveMob(List<MobConfigModel> mobsToRemove, MobConfigModel mob,List<NpcColiseumAssetModel> npcAsset)
        {

            if (mob.CurrentAction == Enums.Map.MobActionEnum.Destroy && DateTime.Now >= mob.DieTime.AddSeconds(5))
            {
                ColiseumStageClear(mob,npcAsset);
                mobsToRemove.Add(mob);
            }

        }

        private void ColiseumStageClear( MobConfigModel mob, List<NpcColiseumAssetModel> npcAsset)
        {
            if (ColiseumMobs.Contains((int)mob.Id))
            {
               ColiseumMobs.Remove((int)mob.Id);

                if (ColiseumMobs.Count == 1)
                {
                    var npcInfo = npcAsset.FirstOrDefault(x => x.NpcId == ColiseumMobs.First());

                    if (npcInfo != null)
                    {
                        foreach (var player in Clients.Where(x => x.Tamer.Partner.Alive))
                        {
                            player.Tamer.Points.IncreaseAmount(npcInfo.MobInfo[player.Tamer.Points.CurrentStage - 1].WinPoints);
                            player?.Send(new DungeonArenaStageClearPacket(mob.Type, mob.TargetTamer.Points.CurrentStage, mob.TargetTamer.Points.Amount, npcInfo.MobInfo[mob.TargetTamer.Points.CurrentStage -1].WinPoints, ColiseumMobs.First()));

                        }

                    }
                }
            }
        }

        private void CheckRemoveMob(List<SummonMobModel> mobsToRemove, SummonMobModel mob)
        {
            if (mob.RemainingMinutes == 0 || mob.CurrentAction == Enums.Map.MobActionEnum.Destroy)
            {
                foreach (var view in mob.TamersViewing)
                {
                    var targetClient = Clients.FirstOrDefault(x => x.TamerId == view);

                    if (targetClient != null)
                    {
                        targetClient.Tamer.RemoveTarget(mob);


                        if (!mob.Dead)
                        {
                            targetClient.Tamer.StopBattle(true);

                            BroadcastForTargetTamers(mob.TamersViewing, new SetCombatOffPacket(targetClient.Tamer.GeneralHandler).Serialize());
                            BroadcastForTargetTamers(mob.TamersViewing, new SetCombatOffPacket(targetClient.Tamer.Partner.GeneralHandler).Serialize());

                            targetClient.Send(new DestroyMobsPacket(mob));
                        }
                    }
                }

                mob.Destroy();

                mobsToRemove.Add(mob);
            }
        }

        public void AttackTarget(MobConfigModel mob, List<NpcColiseumAssetModel> npcAsset)
        {
            #region Hit Damage
            var baseDamage = mob.ATValue - mob.Target.DE + UtilitiesFunctions.RandomInt(1, 15);
            if (baseDamage < 0) baseDamage = 0;

            var critBonusMultiplier = 0.00;
            double critChance = mob.CTValue / 100;
            if (critChance >= UtilitiesFunctions.RandomInt(100))
                critBonusMultiplier = 0.01; //TODO: externalizar no portal

            var blocked = mob.Target.BL >= UtilitiesFunctions.RandomDouble();

            var levelBonusMultiplier = mob.Level > mob.Target.Level ?
                (0.01f * (mob.Level - mob.Target.Level)) : 0; //TODO: externalizar no portal

            var attributeMultiplier = 0.00;
            if (mob.Attribute.HasAttributeAdvantage(mob.Target.BaseInfo.Attribute))
                attributeMultiplier = 0.25;
            else if (mob.Target.BaseInfo.Attribute.HasAttributeAdvantage(mob.Attribute))
                attributeMultiplier = -0.25;

            var elementMultiplier = 0.00;
            if (mob.Element.HasElementAdvantage(mob.Target.BaseInfo.Element))
                elementMultiplier = 0.25;
            else if (mob.Target.BaseInfo.Element.HasElementAdvantage(mob.Element))
                elementMultiplier = -0.25;

            baseDamage /= blocked ? 2 : 1;

            var finalDmg = (int)Math.Floor(baseDamage +
                (baseDamage * critBonusMultiplier) +
                (baseDamage * levelBonusMultiplier) +
                (baseDamage * attributeMultiplier) +
                (baseDamage * elementMultiplier));
            #endregion

            if (finalDmg <= 0) finalDmg = 1;

            var previousHp = mob.Target.CurrentHp;
            var newHp = mob.Target.ReceiveDamage(finalDmg);

            var hitType = blocked ? 2 : critBonusMultiplier > 0 ? 1 : 0;

            if (newHp > 0)
            {
                BroadcastForTargetTamers(mob.TamersViewing, new HitPacket(mob.GeneralHandler, mob.TargetHandler, finalDmg, previousHp, newHp, hitType).Serialize());
                //BroadcastForTargetTamers(mob.TamersViewing, new UpdateCurrentHPRatePacket(mob.TargetHandler, mob.Target.HpRate).Finalize());
            }
            else
            {
                BroadcastForTargetTamers(mob.TamersViewing, new KillOnHitPacket(mob.GeneralHandler, mob.TargetHandler, finalDmg, hitType).Serialize());
                BroadcastForTargetTamers(mob.TamersViewing, new SetCombatOffPacket(mob.TargetHandler).Serialize());

                if (mob.TargetTamer != null)
                {
                    if (ColiseumMobs.Contains((int)mob.Id))
                    {
                        var npcInfo = npcAsset.FirstOrDefault(x => x.NpcId == ColiseumMobs.First());

                        if (npcInfo != null)
                        {
                            mob.TargetTamer.Points.ReductionAmount(npcInfo.MobInfo[mob.TargetTamer.Points.CurrentStage - 1].LosePoints); 
                        }
                    }
                }
                mob.TargetTamer?.Die();
                mob.NextTarget();
            }

            mob.UpdateLastHit();
            mob.UpdateLastHitTry();
        }
        public void AttackTarget(SummonMobModel mob)
        {
            #region Hit Damage
            var baseDamage = mob.ATValue - mob.Target.DE + UtilitiesFunctions.RandomInt(1, 15);
            if (baseDamage < 0) baseDamage = 0;

            var critBonusMultiplier = 0.00;
            double critChance = mob.CTValue / 100;
            if (critChance >= UtilitiesFunctions.RandomInt(100))
                critBonusMultiplier = 0.01; //TODO: externalizar no portal

            var blocked = mob.Target.BL >= UtilitiesFunctions.RandomDouble();

            var levelBonusMultiplier = mob.Level > mob.Target.Level ?
                (0.01f * (mob.Level - mob.Target.Level)) : 0; //TODO: externalizar no portal

            var attributeMultiplier = 0.00;
            if (mob.Attribute.HasAttributeAdvantage(mob.Target.BaseInfo.Attribute))
                attributeMultiplier = 0.25;
            else if (mob.Target.BaseInfo.Attribute.HasAttributeAdvantage(mob.Attribute))
                attributeMultiplier = -0.25;

            var elementMultiplier = 0.00;
            if (mob.Element.HasElementAdvantage(mob.Target.BaseInfo.Element))
                elementMultiplier = 0.25;
            else if (mob.Target.BaseInfo.Element.HasElementAdvantage(mob.Element))
                elementMultiplier = -0.25;

            baseDamage /= blocked ? 2 : 1;

            var finalDmg = (int)Math.Floor(baseDamage +
                (baseDamage * critBonusMultiplier) +
                (baseDamage * levelBonusMultiplier) +
                (baseDamage * attributeMultiplier) +
                (baseDamage * elementMultiplier));
            #endregion

            if (finalDmg <= 0) finalDmg = 1;

            var previousHp = mob.Target.CurrentHp;
            var newHp = mob.Target.ReceiveDamage(finalDmg);

            var hitType = blocked ? 2 : critBonusMultiplier > 0 ? 1 : 0;

            if (newHp > 0)
            {
                BroadcastForTargetTamers(mob.TamersViewing, new HitPacket(mob.GeneralHandler, mob.TargetHandler, finalDmg, previousHp, newHp, hitType).Serialize());
                //BroadcastForTargetTamers(mob.TamersViewing, new UpdateCurrentHPRatePacket(mob.TargetHandler, mob.Target.HpRate).Finalize());
            }
            else
            {
                BroadcastForTargetTamers(mob.TamersViewing, new KillOnHitPacket(mob.GeneralHandler, mob.TargetHandler, finalDmg, hitType).Serialize());
                BroadcastForTargetTamers(mob.TamersViewing, new SetCombatOffPacket(mob.TargetHandler).Serialize());

                mob.TargetTamer?.Die();
                mob.NextTarget();
            }

            mob.UpdateLastHit();
            mob.UpdateLastHitTry();
        }
        /// <summary>
        /// Roll one effect-value sample from the bin's [MinValue..MaxValue] range
        /// (s_dwEff_Val_Min / s_dwEff_Val_Max).  Pre-Step-8 the server always used
        /// MaxValue as a flat number; Step 8 honours the bin's actual range.
        /// </summary>
        private static int RollMonsterSkillValue(MonsterSkillInfoAssetModel skill)
        {
            if (skill.MaxValue <= skill.MinValue) return skill.MaxValue;
            return Random.Shared.Next(skill.MinValue, skill.MaxValue + 1);
        }

        // eEFFECT_TYPE values mirrored from CsMonsterSkill (Monster.h:126-159) +
        // newer values from common_vs2019/cSkillSource.h:194-230 (eMon_SkillEffect).
        // Server uses bare integer constants per the no-enums-on-server convention —
        // enums live in BinTool only.
        private const int EffectHpPerIncrease       = 1;    // self-heal: HPValue * roll/100
        private const int EffectHpValIncrease       = 3;    // self-heal: flat roll
        private const int EffectHpValDecrease       = 4;    // damage in AoE around self
        private const int EffectDsValDecrease       = 10;   // DS drain
        private const int EffectBuffOccure          = 21;   // apply buff (self or target)
        private const int EffectSingleStackDebuff   = 22;   // single-target damage + debuff stack
        private const int EffectExterminate         = 30;   // map-wide damage (no distance gate)
        private const int EffectLegacyHardcoded     = 27045;// pre-bin-migration DB synonym for case 4

        // Default monster-cast buff/debuff duration when the bin row's MaxValue is 0
        // — the bin sometimes carries 0 for "use the buff's own default", which we
        // don't have visibility into without loading Buff.bin into this scope.  Picking
        // 15 s as a sane mid-fight default; can be made bin-driven later if needed.
        private const int DefaultMonsterCastBuffMs  = 15_000;

        public async void SkillTarget(MobConfigModel mob, MonsterSkillInfoAssetModel? targetSkill, List<NpcColiseumAssetModel> npcAsset)
        {
            if (targetSkill == null) return;

            switch (targetSkill.SkillType)
            {
                // ─── Self-heal flat (eEFFECT_TYPE = 3) ────────────────────
                // Mob restores roll(MinValue, MaxValue) HP, capped at HPValue.
                // No broadcast — client refreshes mob HP via next sync tick.
                case EffectHpValIncrease:
                {
                    var heal = RollMonsterSkillValue(targetSkill);
                    if (heal > 0)
                    {
                        var newHp = mob.CurrentHP + heal;
                        if (newHp > mob.HPValue) newHp = mob.HPValue;
                        mob.UpdateCurrentHp(newHp);
                    }
                    break;
                }

                // ─── Self-heal percent (eEFFECT_TYPE = 1) ─────────────────
                // Restore HPValue * roll(MinValue, MaxValue) / 100 (percent).  v487
                // bin has 1 row of this type — usually low-volume boss self-regen.
                case EffectHpPerIncrease:
                {
                    var percent = RollMonsterSkillValue(targetSkill);
                    if (percent > 0)
                    {
                        var heal = (int)((long)mob.HPValue * percent / 100);
                        var newHp = mob.CurrentHP + heal;
                        if (newHp > mob.HPValue) newHp = mob.HPValue;
                        mob.UpdateCurrentHp(newHp);
                    }
                    break;
                }

                // ─── Buff occurrence (eEFFECT_TYPE = 21) ──────────────────
                // Apply a buff identified by MinValue (BuffId), with duration MaxValue
                // (ms; falls back to DefaultMonsterCastBuffMs if zero).  ActiveType
                // routes the target:
                //   0 = self    — buff broadcast against the mob's own handler
                //   1 = target  — broadcast against each target tamer's partner; the
                //                 partner's BuffList also gets the entry server-side
                //                 so subsequent buff queries / sync see it.
                case EffectBuffOccure:
                {
                    int buffId = (int)targetSkill.MinValue;
                    if (buffId <= 0) break;
                    int duration = targetSkill.MaxValue > 0 ? targetSkill.MaxValue : DefaultMonsterCastBuffMs;
                    int skillCode = targetSkill.SkillId;

                    BroadcastForTargetTamers(mob.TamersViewing,
                        new MonsterSkillVisualPacket(mob.GeneralHandler, targetSkill.SkillId).Serialize());

                    if (targetSkill.ActiveType == 1)
                    {
                        // Apply to every in-range target tamer's partner.
                        var copy = new List<CharacterModel>(mob.TargetTamers);
                        foreach (var target in copy)
                        {
                            var clientToModify = Clients.FirstOrDefault(x => x.Tamer.Partner.Id == target.Partner.Id);
                            if (clientToModify == null) continue;
                            var d = UtilitiesFunctions.CalculateDistance(
                                mob.CurrentLocation.X, clientToModify.Partner.Location.X,
                                mob.CurrentLocation.Y, clientToModify.Partner.Location.Y);
                            if (d > 1900) continue;
                            clientToModify.Partner.BuffList.Add(DigimonBuffModel.Create(buffId, skillCode, 0, duration));
                            BroadcastForTargetTamers(mob.TamersViewing,
                                new AddBuffPacket(clientToModify.Partner.GeneralHandler, buffId, skillCode, 0, duration).Serialize());
                        }
                    }
                    else
                    {
                        // Self-buff: server doesn't track mob buffs (no MobConfigModel.BuffList
                        // yet); broadcast only — clients see the visual + render it on the mob.
                        BroadcastForTargetTamers(mob.TamersViewing,
                            new AddBuffPacket(mob.GeneralHandler, buffId, skillCode, 0, duration).Serialize());
                    }
                    break;
                }

                // ─── Single-target damage + debuff stack (eEFFECT_TYPE = 22) ─
                // The highest-volume effect in v487 (625 bin rows — 30% of all skills).
                // Hits the mob's current target only; applies a debuff from EffectFactor[0].
                // Stack visualisation is client-driven — server fires AddBuffPacket each
                // cast and the client handles the stack count UI.
                case EffectSingleStackDebuff:
                {
                    var target = mob.Target;
                    if (target == null || !target.Alive) break;
                    var clientToModify = Clients.FirstOrDefault(x => x.Tamer.Partner.Id == target.Id);
                    if (clientToModify == null) break;

                    var d = UtilitiesFunctions.CalculateDistance(
                        mob.CurrentLocation.X, target.Location.X,
                        mob.CurrentLocation.Y, target.Location.Y);
                    if (d > 1900) break;

                    var dmg = RollMonsterSkillValue(targetSkill);
                    var newHp = target.ReceiveDamage(dmg);
                    if (newHp <= 0) clientToModify.Partner.Die();

                    var hpRate = (byte)((long)target.CurrentHp * 255L / Math.Max(1, target.HP));
                    BroadcastForTargetTamers(mob.TamersViewing,
                        new MonsterSkillVisualPacket(mob.GeneralHandler, targetSkill.SkillId).Serialize());
                    BroadcastForTargetTamers(mob.TamersViewing,
                        new SkillHitPacket(mob.GeneralHandler, target.GeneralHandler, 0, dmg, hpRate).Serialize());

                    // Debuff stack — EffectFactor[0] = debuff BuffId, EffectFactorValue[0] = duration ms.
                    int debuffId = targetSkill.EffectFactor.Length > 0 ? targetSkill.EffectFactor[0] : 0;
                    if (debuffId > 0)
                    {
                        int debuffMs = targetSkill.EffectFactorValue.Length > 0
                            ? (int)Math.Min(targetSkill.EffectFactorValue[0], (uint)int.MaxValue)
                            : 0;
                        if (debuffMs == 0) debuffMs = DefaultMonsterCastBuffMs;

                        target.DebuffList.Add(DigimonDebuffModel.Create(debuffId, targetSkill.SkillId, 1, debuffMs));
                        BroadcastForTargetTamers(mob.TamersViewing,
                            new AddBuffPacket(target.GeneralHandler, debuffId, targetSkill.SkillId, 1, debuffMs).Serialize());
                    }
                    break;
                }

                // ─── Map-wide damage (eEFFECT_TYPE = 30 Exterminate) ──────
                // Same as HP_VAL_DECREASE but no distance gate — every player on the
                // map gets hit.  Wipe-mechanic boss attacks ("global annihilation").
                case EffectExterminate:
                {
                    var dmg = RollMonsterSkillValue(targetSkill);
                    BroadcastForTargetTamers(mob.TamersViewing,
                        new MonsterSkillVisualPacket(mob.GeneralHandler, targetSkill.SkillId).Serialize());
                    foreach (var c in Clients.ToList())
                    {
                        var partner = c.Tamer?.Partner;
                        if (partner == null || !partner.Alive) continue;
                        if (partner.Location.MapId != mob.Location.MapId) continue;
                        var newHp = partner.ReceiveDamage(dmg);
                        var hpRate = (byte)((long)partner.CurrentHp * 255L / Math.Max(1, partner.HP));
                        BroadcastForTargetTamers(mob.TamersViewing,
                            new SkillHitPacket(mob.GeneralHandler, partner.GeneralHandler, 0, dmg, hpRate).Serialize());
                        if (newHp <= 0) partner.Die();
                    }
                    break;
                }

                // ─── DS drain on target tamers (eEFFECT_TYPE = 10) ────────
                case EffectDsValDecrease:
                {
                    var drain = RollMonsterSkillValue(targetSkill);
                    if (drain > 0)
                    {
                        var targetsCopy = new List<CharacterModel>(mob.TargetTamers);
                        foreach (var target in targetsCopy)
                        {
                            var clientToModify = Clients.FirstOrDefault(x => x.Tamer.Partner.Id == target.Partner.Id);
                            if (clientToModify == null) continue;
                            var d = UtilitiesFunctions.CalculateDistance(
                                mob.CurrentLocation.X, clientToModify.Partner.Location.X,
                                mob.CurrentLocation.Y, clientToModify.Partner.Location.Y);
                            if (d <= 1900) clientToModify.Partner.UseDs(drain);
                        }
                        BroadcastForTargetTamers(mob.TamersViewing,
                            new MonsterSkillVisualPacket(mob.GeneralHandler, targetSkill.SkillId).Serialize());
                    }
                    break;
                }

                // ─── Damage to all in-range targets (eEFFECT_TYPE = 4, legacy 27045) ──
                case EffectHpValDecrease:
                case EffectLegacyHardcoded:
                    {
                        List<CharacterModel> targetTamers = new List<CharacterModel>();

                        var finalDamage = RollMonsterSkillValue(targetSkill);

                        // Crie uma cópia da lista mob.TargetTamers para iterar sobre ela
                        var targetTamersCopy = new List<CharacterModel>(mob.TargetTamers);

                        foreach (var target in targetTamersCopy)
                        {
                            var clientToModify = Clients.FirstOrDefault(x => x.Tamer.Partner.Id == target.Partner.Id);

                            if (clientToModify != null)
                            {
                                var diff = UtilitiesFunctions.CalculateDistance(
                                    mob.CurrentLocation.X,
                                  clientToModify.Partner.Location.X,
                                    mob.CurrentLocation.Y,
                                    clientToModify.Partner.Location.Y);


                                if (diff <= 1900)
                                {
                                    var newHp = clientToModify.Partner.ReceiveDamage(finalDamage);

                                    targetTamers.Add(target);

                                    if (newHp <= 0)
                                    {
                                        if (mob.TargetTamer != null)
                                        {
                                            if (ColiseumMobs.Contains((int)mob.Id))
                                            {
                                                var npcInfo = npcAsset.FirstOrDefault(x => x.NpcId == ColiseumMobs.First());

                                                if (npcInfo != null)
                                                {
                                                    clientToModify.Tamer.Points.ReductionAmount(npcInfo.MobInfo[mob.TargetTamer.Points.CurrentStage - 1].LosePoints);
                                                }
                                            }
                                        }

                                        clientToModify?.Partner.Die();

                                    }
                                }
                            }

                            if (!targetTamers.Any())
                            {
                                ChaseTarget(mob);
                            }
                            else
                            {
                                // TODO: Fornecer tempo de espera da skill.

                                BroadcastForTargetTamers(mob.TamersViewing, new MonsterSkillVisualPacket(mob.GeneralHandler, targetSkill.SkillId).Serialize());

                                // Per-target damage via pSkill::ApplyAround (1102) — the v487 client
                                // route for "hitter applies skill to target". The previous code sent
                                // a single MonsterSkillDamagePacket (16011) which v487 routes to
                                // RecvRaidChainSkill (Qinglongmon chain-lightning visual) — wrong shape,
                                // dropped. Server-side HP was decrementing but client showed no damage.
                                foreach (var hitTarget in targetTamers)
                                {
                                    var hpRate = (byte)((long)hitTarget.Partner.CurrentHp * 255L / Math.Max(1, hitTarget.Partner.HP));
                                    BroadcastForTargetTamers(mob.TamersViewing, new SkillHitPacket(
                                        mob.GeneralHandler, hitTarget.Partner.GeneralHandler, 0, finalDamage, hpRate).Serialize());
                                }

                                Task.Run(() =>
                                {
                                    Thread.Sleep((int)targetSkill.AnimationDelay);
                                });


                            }
                        }
                    }
                    break;
            }

            mob.SetSkillCooldown(targetSkill.Cooldown);
            mob.UpdateLastSkill();
            mob.UpdateLastSkillTry();

            if (!mob.Target.Alive)
            {
                mob.NextTarget();
            }

        }
        public async void SkillTarget(SummonMobModel mob, MonsterSkillInfoAssetModel? targetSkill)
        {
            if (targetSkill == null) return;

            switch (targetSkill.SkillType)
            {
                // Damage path — case 4 (HP_VAL_DECREASE) is the bin value; 27045 stays
                // as a legacy synonym for pre-migration DB rows.  Summon mobs only fire
                // damage skills in v487 (no self-heal / DS-drain implementations for them).
                case EffectHpValDecrease:
                case EffectLegacyHardcoded:
                    {
                        List<CharacterModel> targetTamers = new List<CharacterModel>();

                        var finalDamage = RollMonsterSkillValue(targetSkill);

                        // Crie uma cópia da lista mob.TargetTamers para iterar sobre ela
                        var targetTamersCopy = new List<CharacterModel>(mob.TargetTamers);

                        foreach (var target in targetTamersCopy)
                        {
                            var clientToModify = Clients.FirstOrDefault(x => x.Tamer.Partner.Id == target.Partner.Id).Partner;

                            if (clientToModify != null)
                            {
                                var diff = UtilitiesFunctions.CalculateDistance(
                                    mob.CurrentLocation.X,
                                  clientToModify.Location.X,
                                    mob.CurrentLocation.Y,
                                    clientToModify.Location.Y);


                                if (diff <= 1900)
                                {
                                    var newHp = clientToModify.ReceiveDamage(finalDamage);

                                    targetTamers.Add(target);

                                    if (newHp <= 0)
                                    {
                                        clientToModify?.Die();

                                    }
                                }
                            }

                            if (!targetTamers.Any())
                            {
                                ChaseTarget(mob);
                            }
                            else
                            {
                                // TODO: Fornecer tempo de espera da skill.

                                BroadcastForTargetTamers(mob.TamersViewing, new MonsterSkillVisualPacket(mob.GeneralHandler, targetSkill.SkillId).Serialize());

                                // Per-target damage via pSkill::ApplyAround (1102) — the v487 client
                                // route for "hitter applies skill to target". The previous code sent
                                // a single MonsterSkillDamagePacket (16011) which v487 routes to
                                // RecvRaidChainSkill (Qinglongmon chain-lightning visual) — wrong shape,
                                // dropped. Server-side HP was decrementing but client showed no damage.
                                foreach (var hitTarget in targetTamers)
                                {
                                    var hpRate = (byte)((long)hitTarget.Partner.CurrentHp * 255L / Math.Max(1, hitTarget.Partner.HP));
                                    BroadcastForTargetTamers(mob.TamersViewing, new SkillHitPacket(
                                        mob.GeneralHandler, hitTarget.Partner.GeneralHandler, 0, finalDamage, hpRate).Serialize());
                                }

                                Task.Run(() =>
                                {
                                    Thread.Sleep((int)targetSkill.AnimationDelay);
                                });


                            }
                        }
                    }
                    break;
            }

            mob.SetSkillCooldown(targetSkill.Cooldown);
            mob.UpdateLastSkill();
            mob.UpdateLastSkillTry();

            if (!mob.Target.Alive)
            {
                mob.NextTarget();
            }

        }
        public void ChaseTarget(MobConfigModel mob)
        {
            var diff = UtilitiesFunctions.CalculateDistance(
                mob.CurrentLocation.X,
                mob.InitialLocation.X,
                mob.CurrentLocation.Y,
                mob.InitialLocation.Y);

            if (diff >= mob.HuntRange)
            {
                mob.GiveUp();
                return;
            }

            #region Get New Position
            var mobX = mob.CurrentLocation.X;
            var mobY = mob.CurrentLocation.Y;
            var partX = mob.Target.Location.X;
            var partY = mob.Target.Location.Y;

            var deltaX = partX - mobX;
            var deltaY = partY - mobY;

            var distance = Math.Sqrt(Math.Pow(deltaX, 2) + Math.Pow(deltaY, 2));
            var range = Math.Max(mob.ARValue, mob.Target.BaseInfo.ARValue);
            var min_distance = distance - range;
            var angle = Math.Atan2(deltaY, deltaX);

            var newX = (int)Math.Floor(mobX + min_distance * Math.Cos(angle));
            var newY = (int)Math.Floor(mobY + min_distance * Math.Sin(angle));
            #endregion

            var diffNew = (int)UtilitiesFunctions.CalculateDistance(
                mob.CurrentLocation.X,
                newX,
                mob.CurrentLocation.Y,
                newY);

            var wait = diffNew / mob.MSValue * 1000;
            mob.UpdateChaseTime(DateTime.Now.AddMilliseconds(1000 + wait));
            mob.MoveTo(newX, newY);
            BroadcastForTargetTamers(mob.TamersViewing, new MobRunPacket(mob).Serialize());
        }
        public void ChaseTarget(SummonMobModel mob)
        {
            var diff = UtilitiesFunctions.CalculateDistance(
                mob.CurrentLocation.X,
                mob.InitialLocation.X,
                mob.CurrentLocation.Y,
                mob.InitialLocation.Y);

            if (diff >= mob.HuntRange)
            {
                mob.GiveUp();
                return;
            }

            #region Get New Position
            var mobX = mob.CurrentLocation.X;
            var mobY = mob.CurrentLocation.Y;
            var partX = mob.Target.Location.X;
            var partY = mob.Target.Location.Y;

            var deltaX = partX - mobX;
            var deltaY = partY - mobY;

            var distance = Math.Sqrt(Math.Pow(deltaX, 2) + Math.Pow(deltaY, 2));
            var range = Math.Max(mob.ARValue, mob.Target.BaseInfo.ARValue);
            var min_distance = distance - range;
            var angle = Math.Atan2(deltaY, deltaX);

            var newX = (int)Math.Floor(mobX + min_distance * Math.Cos(angle));
            var newY = (int)Math.Floor(mobY + min_distance * Math.Sin(angle));
            #endregion

            var diffNew = (int)UtilitiesFunctions.CalculateDistance(
                mob.CurrentLocation.X,
                newX,
                mob.CurrentLocation.Y,
                newY);

            var wait = diffNew / mob.MSValue * 1000;
            mob.UpdateChaseTime(DateTime.Now.AddMilliseconds(1000 + wait));
            mob.MoveTo(newX, newY);
            BroadcastForTargetTamers(mob.TamersViewing, new MobRunPacket(mob).Serialize());
        }
        public void AttackNearbyTamer(MobConfigModel mob, List<long> nearbyTamers, List<NpcColiseumAssetModel> npcAsset)
        {
            if (mob.ReactionType == DigimonReactionTypeEnum.Agressive && !mob.Dead && !mob.InBattle && DateTime.Now > mob.AgressiveCheckTime && nearbyTamers.Any())
            {
                foreach (var tamerId in nearbyTamers)
                {
                    var targetClient = Clients.FirstOrDefault(x => x.TamerId == tamerId);
                    if (targetClient == null || targetClient.Tamer.Hidden || targetClient.Tamer.Dead || targetClient.Tamer.Riding)
                        continue;

                    var diff = UtilitiesFunctions.CalculateDistance(targetClient.Tamer.Partner.Location.X,
                        mob.CurrentLocation.X,
                        targetClient.Tamer.Partner.Location.Y,
                        mob.CurrentLocation.Y);

                    if (diff <= mob.ViewRange)
                    {
                        BroadcastForTargetTamers(mob.TamersViewing, new MobTinklePacket(mob.GeneralHandler).Serialize());
                        BroadcastForTargetTamers(mob.TamersViewing, new SetCombatOnPacket(mob.GeneralHandler).Serialize());
                        BroadcastForTargetTamers(mob.TamersViewing, new SetCombatOnPacket(targetClient.Partner.GeneralHandler).Serialize());

                        mob.StartBattle(targetClient.Tamer);
                        targetClient.Tamer.StartBattle(mob);

                        if (!targetClient.Tamer.GodMode)
                            AttackTarget(mob, npcAsset);

                        break;
                    }
                }
            }
        }

        #region Handler
        private bool NeedNewHandler(MobConfigModel mob)
        {
            return MobHandlers.Values.FirstOrDefault(x => x == mob.Id) == 0;
        }

        public void SetMobHandler(MobConfigModel mob)
        {
            short handler = 0;

            lock (MobHandlers)
            {
                FreeMobHandler(mob.Id);

                handler = MobHandlers.First(x => x.Value == 0).Key;

                MobHandlers[handler] = mob.Id;
            }

            mob.SetHandlerValue(handler);
        }

        private bool NeedNewHandler(SummonMobModel mob)
        {
            return MobHandlers.Values.FirstOrDefault(x => x == mob.Id) == 0;
        }

        public void SetMobHandler(SummonMobModel mob)
        {
            short handler = 0;

            lock (MobHandlers)
            {
                FreeMobHandler(mob.Id);

                handler = MobHandlers.First(x => x.Value == 0).Key;

                MobHandlers[handler] = mob.Id;
            }

            mob.SetHandlerValue(handler);
        }

        public void FreeMobHandler(long mobId)
        {
            lock (MobHandlers)
            {
                var handler = MobHandlers.FirstOrDefault(x => x.Value == mobId).Key;

                if (handler > 0)
                    MobHandlers[handler] = 0;
            }
        }
        #endregion

        #region View
        private void ResetView(long tamerTarget)
        {
            lock (MobsView)
            {
                var mobViewList = MobsView.Where(x => x.Value.Contains(tamerTarget));

                foreach (var mobView in mobViewList)
                {
                    mobView.Value.Remove(tamerTarget);
                }
            }
        }
        #endregion

        public void RemoveMob(MobConfigModel mob)
        {
            lock (Mobs)
            {
                if (Mobs.Contains(mob))
                {
                    FreeMobHandler(mob.Id);
                    Mobs.Remove(mob);
                }
            }
        }

        public void AddMob(MobConfigModel mob)
        {
            lock (Mobs)
            {
                if (!Mobs.Exists(x => x.Id == mob.Id))
                {
                    if (NeedNewHandler(mob))
                        SetMobHandler(mob);

                    mob.SetInitialLocation();
                    mob.UpdateCurrentHp(mob.HPValue);

                    Mobs.Add(mob);
                }
            }
        }
        public void RemoveMob(SummonMobModel mob)
        {
            lock (SummonMobs)
            {
                if (SummonMobs.Contains(mob))
                {
                    FreeMobHandler(mob.Id);
                    SummonMobs.Remove(mob);
                }
            }
        }

        public void AddMob(SummonMobModel mob)
        {
            lock (SummonMobs)
            {
                if (!SummonMobs.Exists(x => x.Id == mob.Id))
                {
                    if (NeedNewHandler(mob))
                        SetMobHandler(mob);

                    mob.SetInitialLocation();
                    mob.UpdateCurrentHp(mob.HPValue);

                    SummonMobs.Add(mob);
                }
            }
        }

        public void UpdateMobsList()
        {
            //_mobsToAdd.Clear();
            //if (MobsToAdd != null)
            //    _mobsToAdd.AddRange(MobsToAdd);

            _mobsToDestroy.Clear();
            if (MobsToRemove != null)
                _mobsToDestroy.AddRange(MobsToRemove);
        }
    }
}
