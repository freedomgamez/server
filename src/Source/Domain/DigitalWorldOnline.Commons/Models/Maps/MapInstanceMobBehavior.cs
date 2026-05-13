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
    public sealed partial class MapInstance
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

            foreach (var mob in _mobsToAdd)
                AddMob(mob);

            FinishMobsUpdate();
        }
        public void UpdateMapMobs(List<NpcColiseumAssetModel> npcAsset)
        {
            

            var mobsToRemove = new List<MobConfigModel>();

            foreach (var mob in Mobs)
                CheckRemoveMob(mobsToRemove, mob, npcAsset);

            foreach (var mob in mobsToRemove)
                RemoveMob(mob);

            foreach (var mob in _mobsToAdd)
                AddMob(mob);

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

            baseDamage /= blocked ? 2 : 1;
            baseDamage = UtilitiesFunctions.ApplyNatureMatrixDamage(
                baseDamage,
                mob.Attribute,
                mob.Target.BaseInfo.Attribute,
                mob.Element,
                mob.Target.BaseInfo.Element);

            var finalDmg = (int)Math.Floor(baseDamage +
                (baseDamage * critBonusMultiplier) +
                (baseDamage * levelBonusMultiplier));
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

            baseDamage /= blocked ? 2 : 1;
            baseDamage = UtilitiesFunctions.ApplyNatureMatrixDamage(
                baseDamage,
                mob.Attribute,
                mob.Target.BaseInfo.Attribute,
                mob.Element,
                mob.Target.BaseInfo.Element);

            var finalDmg = (int)Math.Floor(baseDamage +
                (baseDamage * critBonusMultiplier) +
                (baseDamage * levelBonusMultiplier));
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
        /// Resolve a skill's AoE radius: prefer the bin-joined value from
        /// <c>Terms[RangeId].Range</c> (now exposed on the DTO as <c>RangeUnits</c>);
        /// fall back to the legacy hardcoded 1900-unit gate when the bin row has no
        /// Terms row (RangeId = 0).  Used by every AoE distance check in the dispatcher
        /// so a Monster.bin tuning change actually reaches the server-side hit-test.
        /// </summary>
        private static int MobSkillRadius(MonsterSkillInfoAssetModel skill)
            => skill.RangeUnits > 0 ? (int)skill.RangeUnits : 1900;

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

        // ─── Pending-summon queue (Phase 5) ──────────────────────────────────
        // SUMMON_MONSTER / SummonPos dispatch can't build a SummonMobModel from here
        // because (a) the catalog (MonsterBinLoader) lives in Application and (b) the
        // broadcast-aware spawn API (_mapServer.AddSummonMobs) lives in Distribution.
        // The dispatcher enqueues a PendingSummon and the map-server drains it.
        private readonly System.Collections.Generic.Queue<PendingSummon> _pendingSummons = new();
        private readonly object _pendingSummonsLock = new();

        public void EnqueueSummon(PendingSummon summon)
        {
            lock (_pendingSummonsLock) _pendingSummons.Enqueue(summon);
        }

        /// <summary>Drain the queue.  Caller (Distribution) builds + spawns each one.</summary>
        public System.Collections.Generic.List<PendingSummon> DrainPendingSummons()
        {
            lock (_pendingSummonsLock)
            {
                if (_pendingSummons.Count == 0) return new System.Collections.Generic.List<PendingSummon>();
                var list = new System.Collections.Generic.List<PendingSummon>(_pendingSummons);
                _pendingSummons.Clear();
                return list;
            }
        }

        // ─── AoE-zone tick infrastructure ────────────────────────────────────
        // Persistent zones spawned by ATTACH_SEED (18), Region_Buff_Nesting (23),
        // RandomAoE (27), CONTINUE_WIDE_ATTACK (20).  Lock guards add/remove against
        // the tick loop which the map-server thread runs once per MonsterOperation cycle.
        private readonly List<MapAoeZone> _aoeZones = new();
        private readonly object _aoeZonesLock = new();

        public void AddAoeZone(MapAoeZone zone)
        {
            lock (_aoeZonesLock) _aoeZones.Add(zone);
        }

        /// <summary>
        /// Tick every active zone.  Called from <c>MapServer.MonsterOperation</c> at the
        /// top of each map cycle.  Zones whose <c>TicksRemaining</c> hit zero (or whose
        /// <c>ExpiresAt</c> elapsed) are removed.
        /// </summary>
        public void TickAoeZones()
        {
            List<MapAoeZone> snapshot;
            lock (_aoeZonesLock) snapshot = _aoeZones.ToList();
            if (snapshot.Count == 0) return;

            var toRemove = new List<MapAoeZone>(snapshot.Count);
            foreach (var z in snapshot)
                if (z.Tick(this)) toRemove.Add(z);

            if (toRemove.Count > 0)
                lock (_aoeZonesLock) foreach (var z in toRemove) _aoeZones.Remove(z);
        }

        /// <summary>
        /// Revert any expired GROWTH / BERSERK stat-mods.  Called from
        /// <c>MapServer.MonsterOperation</c> alongside <see cref="TickAoeZones"/>.
        /// </summary>
        public void TickMobStatMods()
        {
            var now = System.DateTime.Now;
            foreach (var mob in Mobs)
            {
                bool growExpired = mob.GrowStack > 0 && mob.GrowExpiresAt != default && now >= mob.GrowExpiresAt;
                bool berserkExpired = mob.Berserk && now >= mob.BerserkExpiresAt;
                if (!growExpired && !berserkExpired) continue;
                if (growExpired)
                {
                    mob.GrowStack = 0;
                    mob.GrowExpiresAt = default;
                }
                if (berserkExpired)
                {
                    mob.Berserk = false;
                    mob.BerserkReflectDamage = 0;
                    mob.BerserkExpiresAt = default;
                }
                // Only restore stats once both modifiers are off.  If GROWTH is still active
                // while BERSERK expires (or vice versa) we leave the combined snapshot alone —
                // the next stat-mod cast recalculates against BaseATSnapshot/BaseDESnapshot.
                if (mob.GrowStack == 0 && !mob.Berserk) mob.RestoreBaseStats();
            }
        }

        /// <summary>
        /// ASSEMBLE / DISPERSE / GatheringExt / DisperseExt — pick N marks, damage each,
        /// broadcast a single <see cref="MobAreaSkillPacket"/> carrying the mark list
        /// and per-target hit entries.  <paramref name="divideDamage"/> splits the rolled
        /// total across the mark count (ASSEMBLE) vs applying it to each (DISPERSE).
        /// </summary>
        private void ApplyMarkedTargetSkill(MobConfigModel mob, MonsterSkillInfoAssetModel skill, bool divideDamage)
        {
            // Mark count comes from bin's TargetCount, clamped to [TargetMin, TargetMax].
            int desired = skill.TargetCount > 0 ? skill.TargetCount : 1;
            if (skill.TargetMin > 0 && desired < skill.TargetMin) desired = skill.TargetMin;
            if (skill.TargetMax > 0 && desired > skill.TargetMax) desired = skill.TargetMax;
            if (desired <= 0) return;

            // Pick eligible targets (in-range alive partners).
            var alive = new List<DigimonModel>();
            var copy = new List<CharacterModel>(mob.TargetTamers);
            foreach (var t in copy)
            {
                var partner = t.Partner;
                if (partner == null || !partner.Alive) continue;
                var d = UtilitiesFunctions.CalculateDistance(
                    mob.CurrentLocation.X, partner.Location.X,
                    mob.CurrentLocation.Y, partner.Location.Y);
                if (d <= MobSkillRadius(skill)) alive.Add(partner);
            }
            if (alive.Count == 0) return;

            // Take up to `desired` marks — first N by aggro list order.
            int markCount = Math.Min(desired, alive.Count);
            var marks = alive.GetRange(0, markCount);

            int total = RollMonsterSkillValue(skill);
            int perTarget = divideDamage && markCount > 0 ? total / markCount : total;
            if (perTarget < 0) perTarget = 0;

            var markHandlers = new List<int>(markCount);
            var hits = new List<MobAreaSkillPacket.Hit>(markCount);
            foreach (var partner in marks)
            {
                var newHp = partner.ReceiveDamage(perTarget);
                var hpRate = (byte)((long)partner.CurrentHp * 255L / Math.Max(1, partner.HP));
                bool died = newHp <= 0;
                if (died) partner.Die();
                markHandlers.Add(partner.GeneralHandler);
                hits.Add(new MobAreaSkillPacket.Hit(partner.GeneralHandler, perTarget, hpRate, died));
            }

            BroadcastForTargetTamers(mob.TamersViewing,
                new MonsterSkillVisualPacket(mob.GeneralHandler, skill.SkillId).Serialize());
            BroadcastForTargetTamers(mob.TamersViewing,
                new MobAreaSkillPacket(mob.GeneralHandler, skill.SkillId, markHandlers, hits).Serialize());
        }

        // eEFFECT_TYPE values mirrored from CsMonsterSkill (Monster.h:126-159) +
        // newer values from common_vs2019/cSkillSource.h:194-230 (eMon_SkillEffect).
        // Server uses bare integer constants per the no-enums-on-server convention —
        // enums live in BinTool only.
        private const int EffectHpPerIncrease       = 1;    // self-heal: HPValue * roll/100
        private const int EffectHpValIncrease       = 3;    // self-heal: flat roll
        private const int EffectHpValDecrease       = 4;    // damage in AoE around self
        private const int EffectDsValDecrease       = 10;   // DS drain
        private const int EffectSummonMonster       = 13;   // spawn N mobs from catalog
        private const int EffectGrowth              = 14;   // self stacking stat-up (DP/AP)
        private const int EffectCallUp              = 15;   // rally nearby mobs to caster's target
        private const int EffectBerserk             = 19;   // self one-shot stat-up + reflect
        private const int EffectAssemble            = 16;   // mark N targets, divide damage among them
        private const int EffectDisperse            = 17;   // mark N targets, full damage to each
        private const int EffectAttachSeed          = 18;   // drop N "seed" zones at target positions
        private const int EffectContinueWide        = 20;   // caster-anchored AoE, ticks N times
        private const int EffectBuffOccure          = 21;   // apply buff (self or target)
        private const int EffectSingleStackDebuff   = 22;   // single-target damage + debuff stack
        private const int EffectRegionBuffNesting   = 23;   // ground zone — ticks debuff onto in-zone targets
        private const int EffectGatheringExt        = 25;   // ASSEMBLE with projectile VFX (server-identical)
        private const int EffectDisperseExt         = 26;   // DISPERSE with projectile VFX (server-identical)
        private const int EffectRandomAoE           = 27;   // pick N random positions, one-tick damage in each
        private const int EffectChainBounce         = 28;   // Qinglongmon-style chain lightning
        private const int EffectExterminate         = 30;   // map-wide damage (no distance gate)
        private const int EffectSummonPos           = 31;   // spawn N mobs at bin-fixed coord
        private const int EffectLegacyHardcoded     = 27045;// pre-bin-migration DB synonym for case 4

        // Once-per-SkillType warning log for SkillTypes not covered by any case arm.
        // Without this, an unrecognised value would silently no-op and we'd never know
        // the bin had a row we hadn't implemented.  HashSet is process-local — restarts
        // re-emit; that's fine, it's a dev signal.
        private static readonly System.Collections.Generic.HashSet<int> _warnedUnhandledMobSkillTypes = new();
        private static readonly object _warnedUnhandledLock = new();

        // Default monster-cast buff/debuff duration when the bin row's MaxValue is 0
        // — the bin sometimes carries 0 for "use the buff's own default", which we
        // don't have visibility into without loading Buff.bin into this scope.  Picking
        // 15 s as a sane mid-fight default; can be made bin-driven later if needed.
        private const int DefaultMonsterCastBuffMs  = 15_000;

        private static int ResolveFallbackDamage(MonsterSkillInfoAssetModel skill)
        {
            int min = skill.MinValue;
            int max = skill.MaxValue;

            if (min > 0 && max >= min)
                return min + ((max - min) / 2);
            if (max > 0)
                return max;
            if (min > 0)
                return min;
            return 1;
        }

        private static void WarnUnhandledSkillTypeOnce(int skillType, int skillId, int mobType)
        {
            bool emit = false;
            lock (_warnedUnhandledLock)
            {
                if (_warnedUnhandledMobSkillTypes.Add(skillType)) emit = true;
            }

            if (emit)
                System.Console.WriteLine(
                    $"[MobSkillDispatch] Unhandled SkillType={skillType} (skillId={skillId}, mobType={mobType}) — fallback damage path engaged.");
        }

        private void ApplyFallbackDamageSkill(MobConfigModel mob, MonsterSkillInfoAssetModel targetSkill, int damage)
        {
            List<CharacterModel> targetTamers = new();
            var targetTamersCopy = new List<CharacterModel>(mob.TargetTamers);

            foreach (var target in targetTamersCopy)
            {
                var clientToModify = Clients.FirstOrDefault(x => x.Tamer.Partner.Id == target.Partner.Id);
                if (clientToModify == null) continue;

                var diff = UtilitiesFunctions.CalculateDistance(
                    mob.CurrentLocation.X,
                    clientToModify.Partner.Location.X,
                    mob.CurrentLocation.Y,
                    clientToModify.Partner.Location.Y);

                if (diff > MobSkillRadius(targetSkill))
                    continue;

                var newHp = clientToModify.Partner.ReceiveDamage(damage);
                targetTamers.Add(target);

                if (newHp <= 0)
                    clientToModify.Partner.Die();
            }

            if (!targetTamers.Any())
            {
                ChaseTarget(mob);
                return;
            }

            BroadcastForTargetTamers(mob.TamersViewing, new MonsterSkillVisualPacket(mob.GeneralHandler, targetSkill.SkillId).Serialize());

            foreach (var hitTarget in targetTamers)
            {
                var hpRate = (byte)((long)hitTarget.Partner.CurrentHp * 255L / Math.Max(1, hitTarget.Partner.HP));
                BroadcastForTargetTamers(mob.TamersViewing, new SkillHitPacket(
                    mob.GeneralHandler, hitTarget.Partner.GeneralHandler, 0, damage, hpRate).Serialize());
            }
        }

        private void ApplyFallbackDamageSkill(SummonMobModel mob, MonsterSkillInfoAssetModel targetSkill, int damage)
        {
            List<CharacterModel> targetTamers = new();
            var targetTamersCopy = new List<CharacterModel>(mob.TargetTamers);

            foreach (var target in targetTamersCopy)
            {
                var targetPartner = Clients.FirstOrDefault(x => x.Tamer.Partner.Id == target.Partner.Id)?.Partner;
                if (targetPartner == null) continue;

                var diff = UtilitiesFunctions.CalculateDistance(
                    mob.CurrentLocation.X,
                    targetPartner.Location.X,
                    mob.CurrentLocation.Y,
                    targetPartner.Location.Y);

                if (diff > MobSkillRadius(targetSkill))
                    continue;

                var newHp = targetPartner.ReceiveDamage(damage);
                targetTamers.Add(target);

                if (newHp <= 0)
                    targetPartner.Die();
            }

            if (!targetTamers.Any())
            {
                ChaseTarget(mob);
                return;
            }

            BroadcastForTargetTamers(mob.TamersViewing, new MonsterSkillVisualPacket(mob.GeneralHandler, targetSkill.SkillId).Serialize());

            foreach (var hitTarget in targetTamers)
            {
                var hpRate = (byte)((long)hitTarget.Partner.CurrentHp * 255L / Math.Max(1, hitTarget.Partner.HP));
                BroadcastForTargetTamers(mob.TamersViewing, new SkillHitPacket(
                    mob.GeneralHandler, hitTarget.Partner.GeneralHandler, 0, damage, hpRate).Serialize());
            }
        }

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
                            if (d > MobSkillRadius(targetSkill)) continue;
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
                    if (d > MobSkillRadius(targetSkill)) break;

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

                // ─── ASSEMBLE / DISPERSE / GatheringExt / DisperseExt ─────
                // (eEFFECT_TYPE = 16, 17, 25, 26).  All four share the same client
                // wire format (cCliGameSkill.cpp:251 unifies them in one switch case).
                // Server-side they differ only in damage math:
                //   ASSEMBLE / GatheringExt  → total damage divided among marks
                //   DISPERSE / DisperseExt   → full roll applied to EACH mark
                // 25 vs 16 (and 26 vs 17) differ only in client VFX (projectile vs instant) —
                // that's driven by s_nValocity/s_nAccel which the bin carries and the client
                // reads locally; server treatment is identical.
                case EffectAssemble:
                case EffectDisperse:
                case EffectGatheringExt:
                case EffectDisperseExt:
                {
                    ApplyMarkedTargetSkill(mob, targetSkill,
                        divideDamage: targetSkill.SkillType == EffectAssemble || targetSkill.SkillType == EffectGatheringExt);
                    break;
                }

                // ─── ChainBounce (eEFFECT_TYPE = 28) ────────────────────────
                // Qinglongmon-style chain lightning.  Start at caster's current target,
                // bounce to nearest tamer-partner within Range that isn't already in the
                // chain, up to TargetCount links.  Each link takes damage; the VFX is
                // broadcast as a single chain packet (16027) that the client renders as
                // a sequence of lightning bolts between consecutive pairs.
                case EffectChainBounce:
                {
                    if (mob.Target == null) break;
                    int maxChain = targetSkill.TargetCount > 0 ? targetSkill.TargetCount : 3;
                    int bounceRange = MobSkillRadius(targetSkill);
                    int mapId = mob.Location.MapId;

                    var chain = new List<DigimonModel> { mob.Target };
                    var used = new HashSet<long> { mob.Target.Id };

                    while (chain.Count < maxChain)
                    {
                        var last = chain[chain.Count - 1];
                        DigimonModel? next = null;
                        int bestDist = int.MaxValue;
                        foreach (var c in Clients.ToList())
                        {
                            var partner = c.Tamer?.Partner;
                            if (partner == null || !partner.Alive) continue;
                            if (partner.Location.MapId != mapId) continue;
                            if (used.Contains(partner.Id)) continue;
                            int d = (int)UtilitiesFunctions.CalculateDistance(
                                last.Location.X, partner.Location.X,
                                last.Location.Y, partner.Location.Y);
                            if (d > bounceRange) continue;
                            if (d < bestDist) { bestDist = d; next = partner; }
                        }
                        if (next == null) break;
                        chain.Add(next);
                        used.Add(next.Id);
                    }

                    BroadcastForTargetTamers(mob.TamersViewing,
                        new MonsterSkillVisualPacket(mob.GeneralHandler, targetSkill.SkillId).Serialize());

                    // Roll damage per link (could decay — bin spec is ambiguous, treat each
                    // link as a fresh roll of [MinValue..MaxValue]).
                    foreach (var link in chain)
                    {
                        var dmg = RollMonsterSkillValue(targetSkill);
                        var newHp = link.ReceiveDamage(dmg);
                        var hpRate = (byte)((long)link.CurrentHp * 255L / System.Math.Max(1, (int)link.HP));
                        bool died = newHp <= 0;
                        if (died) link.Die();
                        BroadcastForTargetTamers(mob.TamersViewing,
                            new SkillHitPacket(mob.GeneralHandler, link.GeneralHandler, 0, dmg, hpRate).Serialize());
                    }

                    // Chain-VFX broadcast (cosmetic): the chain packet only renders the
                    // lightning between pairs; needs ≥2 targets to look like a chain.
                    if (chain.Count >= 2)
                    {
                        var handlers = new List<int>(chain.Count);
                        foreach (var link in chain) handlers.Add(link.GeneralHandler);
                        BroadcastForTargetTamers(mob.TamersViewing,
                            new MobChainSkillPacket(mob.GeneralHandler, targetSkill.SkillId, handlers).Serialize());
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
                            if (d <= MobSkillRadius(targetSkill)) clientToModify.Partner.UseDs(drain);
                        }
                        BroadcastForTargetTamers(mob.TamersViewing,
                            new MonsterSkillVisualPacket(mob.GeneralHandler, targetSkill.SkillId).Serialize());
                    }
                    break;
                }

                // ─── SUMMON_MONSTER (eEFFECT_TYPE = 13) ────────────────────
                // Spawn TargetCount new mobs from catalog (MinValue = MonsterId).
                // Anchor: ActiveType 0 = self, 1 = target, 2 = falls back to self
                // (real coord is not in the bin row).  Drained by MapServerMonsterOperation.
                case EffectSummonMonster:
                {
                    int monsterId = (int)targetSkill.MinValue;
                    if (monsterId <= 0) break;
                    int count = targetSkill.TargetCount > 0 ? targetSkill.TargetCount : 1;
                    int ax, ay;
                    switch (targetSkill.ActiveType)
                    {
                        case 1 when mob.Target != null:
                            ax = mob.Target.Location.X; ay = mob.Target.Location.Y; break;
                        default:
                            ax = mob.CurrentLocation.X; ay = mob.CurrentLocation.Y; break;
                    }
                    BroadcastForTargetTamers(mob.TamersViewing,
                        new MonsterSkillVisualPacket(mob.GeneralHandler, targetSkill.SkillId).Serialize());
                    EnqueueSummon(new PendingSummon(monsterId, ax, ay, count, mob.Id, mob.TargetHandler));
                    break;
                }

                // ─── SummonPos (eEFFECT_TYPE = 31) ─────────────────────────
                // Same as SUMMON_MONSTER but bin row's MaxValue carries a packed coord
                // (or two short halves) — v487 has 1 row of this type so the exact
                // packing is uncertain.  Until we see a second row we fall back to
                // anchoring at the caster (same as SUMMON_MONSTER ActiveType=0).
                case EffectSummonPos:
                {
                    int monsterId = (int)targetSkill.MinValue;
                    if (monsterId <= 0) break;
                    int count = targetSkill.TargetCount > 0 ? targetSkill.TargetCount : 1;
                    int ax = mob.CurrentLocation.X, ay = mob.CurrentLocation.Y;
                    BroadcastForTargetTamers(mob.TamersViewing,
                        new MonsterSkillVisualPacket(mob.GeneralHandler, targetSkill.SkillId).Serialize());
                    EnqueueSummon(new PendingSummon(monsterId, ax, ay, count, mob.Id, mob.TargetHandler));
                    break;
                }

                // ─── CALL_UP (eEFFECT_TYPE = 15) ───────────────────────────
                // "MonsterGather" — rally living mobs within range to engage the
                // caster's current target.  No new packets: re-uses the standard
                // aggro path (StartBattle), and the existing run/sync stream pushes
                // each rallied mob's new state to clients via the normal tick loop.
                // RangeIndex → Terms.Range isn't exposed on the DTO yet, so we use
                // a sensible default (2000 units ≈ a Tamer screen-width).
                case EffectCallUp:
                {
                    if (mob.Target == null || mob.TargetTamer == null) break;
                    var caller = mob.TargetTamer;
                    int range = MobSkillRadius(targetSkill);
                    int cap = targetSkill.TargetCount > 0 ? targetSkill.TargetCount : int.MaxValue;
                    int called = 0;
                    foreach (var sibling in Mobs)
                    {
                        if (called >= cap) break;
                        if (sibling == mob || !sibling.Alive) continue;
                        if (sibling.Location.MapId != mob.Location.MapId) continue;
                        if (UtilitiesFunctions.CalculateDistance(
                            mob.CurrentLocation.X, sibling.CurrentLocation.X,
                            mob.CurrentLocation.Y, sibling.CurrentLocation.Y) > range) continue;
                        sibling.StartBattle(caller);
                        called++;
                    }
                    BroadcastForTargetTamers(mob.TamersViewing,
                        new MonsterSkillVisualPacket(mob.GeneralHandler, targetSkill.SkillId).Serialize());
                    break;
                }

                // ─── GROWTH (eEFFECT_TYPE = 14) ────────────────────────────
                // Self stacking stat-up.  EffectFactor[i] selects which stat (29=AP/AT,
                // 21=DP/DE, 41=visual-scale), EffectFactorValue[i] is % bonus per stack.
                // Stack capped at MaxValue; reverts after duration (using MaxValue ms
                // when EffectFactorValue is unit-less is ambiguous — we use a 30 s
                // default + reset on death).
                case EffectGrowth:
                {
                    mob.EnsureStatSnapshot();
                    int cap = mob.HPValue > 0 && targetSkill.MaxValue > 0
                        ? System.Math.Min(targetSkill.MaxValue, byte.MaxValue)
                        : 5;
                    int newStack = System.Math.Min(mob.GrowStack + 1, cap);
                    mob.GrowStack = (byte)newStack;

                    int atBonus = 0, deBonus = 0;
                    for (int i = 0; i < targetSkill.EffectFactor.Length; i++)
                    {
                        int factor = targetSkill.EffectFactor[i];
                        int pct = i < targetSkill.EffectFactorValue.Length ? (int)targetSkill.EffectFactorValue[i] : 0;
                        if (pct == 0) continue;
                        if (factor == 29) atBonus += (int)((long)mob.BaseATSnapshot * pct * newStack / 100);
                        else if (factor == 21) deBonus += (int)((long)mob.BaseDESnapshot * pct * newStack / 100);
                        // factor 41 = client-side scale VFX only
                    }
                    mob.SetAT(mob.BaseATSnapshot + atBonus);
                    mob.SetDE(mob.BaseDESnapshot + deBonus);
                    mob.GrowExpiresAt = System.DateTime.Now.AddMilliseconds(30_000);

                    BroadcastForTargetTamers(mob.TamersViewing,
                        new MonsterSkillVisualPacket(mob.GeneralHandler, targetSkill.SkillId).Serialize());
                    break;
                }

                // ─── BERSERK (eEFFECT_TYPE = 19) ───────────────────────────
                // Self one-shot stat-up (no stacking).  EffectFactor[i]/EffectFactorValue[i]
                // map identically to GROWTH (29=AP %, 21=DP %).  MinValue carries the
                // reflect-damage value (per Monster.h:149 "+ 반사 추가").  MaxValue =
                // duration ms (fallback 30 s).  No new client packet — the visual is
                // driven by the cast animation (ANI::BERSERK_SKILL).
                case EffectBerserk:
                {
                    mob.EnsureStatSnapshot();
                    int atBonus = 0, deBonus = 0;
                    for (int i = 0; i < targetSkill.EffectFactor.Length; i++)
                    {
                        int factor = targetSkill.EffectFactor[i];
                        int pct = i < targetSkill.EffectFactorValue.Length ? (int)targetSkill.EffectFactorValue[i] : 0;
                        if (pct == 0) continue;
                        if (factor == 29) atBonus += (int)((long)mob.BaseATSnapshot * pct / 100);
                        else if (factor == 21) deBonus += (int)((long)mob.BaseDESnapshot * pct / 100);
                    }
                    mob.SetAT(mob.BaseATSnapshot + atBonus);
                    mob.SetDE(mob.BaseDESnapshot + deBonus);
                    mob.Berserk = true;
                    mob.BerserkReflectDamage = (int)targetSkill.MinValue;
                    int duration = targetSkill.MaxValue > 0 ? targetSkill.MaxValue : 30_000;
                    mob.BerserkExpiresAt = System.DateTime.Now.AddMilliseconds(duration);

                    BroadcastForTargetTamers(mob.TamersViewing,
                        new MonsterSkillVisualPacket(mob.GeneralHandler, targetSkill.SkillId).Serialize());
                    break;
                }

                // ─── ATTACH_SEED (eEFFECT_TYPE = 18) ───────────────────────
                // Drop N seed-zones at target tamer positions; each zone ticks damage
                // over its lifetime.  In v487 the client's RecvMonsterSkill_Use (1123)
                // pops (targetUID, posX, posY) pairs — we skip that broadcast (server
                // resolves spatially) and rely on per-tick MobAreaSkillPacket payloads
                // for the visible damage, plus the cast-start visual.  Bin fields:
                //   TargetCount          = seed count (clamped ≥ 1)
                //   EffectFactorValue[0] = total zone lifetime ms (fallback 5000)
                //   EffectFactor[0]      = tick count (fallback 1)
                case EffectAttachSeed:
                {
                    int seedCount = System.Math.Max(1, (int)targetSkill.TargetCount);
                    int lifetimeMs = targetSkill.EffectFactorValue.Length > 0 && targetSkill.EffectFactorValue[0] > 0
                        ? (int)System.Math.Min(targetSkill.EffectFactorValue[0], (uint)int.MaxValue)
                        : 5_000;
                    int tickCount = targetSkill.EffectFactor.Length > 0 && targetSkill.EffectFactor[0] > 0
                        ? targetSkill.EffectFactor[0]
                        : 1;
                    int interval = System.Math.Max(500, lifetimeMs / System.Math.Max(1, tickCount));

                    var aliveTargets = new List<DigimonModel>();
                    foreach (var t in new List<CharacterModel>(mob.TargetTamers))
                    {
                        var p = t.Partner;
                        if (p == null || !p.Alive) continue;
                        if (UtilitiesFunctions.CalculateDistance(
                            mob.CurrentLocation.X, p.Location.X,
                            mob.CurrentLocation.Y, p.Location.Y) <= 3000) aliveTargets.Add(p);
                    }
                    if (aliveTargets.Count == 0) break;

                    BroadcastForTargetTamers(mob.TamersViewing,
                        new MonsterSkillVisualPacket(mob.GeneralHandler, targetSkill.SkillId).Serialize());

                    int seeds = System.Math.Min(seedCount, aliveTargets.Count);
                    int dmg = RollMonsterSkillValue(targetSkill);
                    int casterHandler = mob.GeneralHandler;
                    int skillIndex = targetSkill.SkillId;
                    // ATTACH_SEED radius — bin-driven (Terms.Range).  Each seed is a small
                    // localised AoE, so the bin number is typically smaller than the
                    // wide-AoE radii on cases 20/23.  Fallback uses the legacy 700.
                    int radius = targetSkill.RangeUnits > 0 ? (int)targetSkill.RangeUnits : 700;
                    int mapId = mob.Location.MapId;
                    var viewing = mob.TamersViewing;

                    for (int i = 0; i < seeds; i++)
                    {
                        int cx = aliveTargets[i].Location.X;
                        int cy = aliveTargets[i].Location.Y;
                        var zone = new MapAoeZone(
                            casterHandler, skillIndex, cx, cy, radius,
                            interval, tickCount,
                            (z, map) =>
                            {
                                var hits = new List<MobAreaSkillPacket.Hit>();
                                foreach (var c in map.Clients.ToList())
                                {
                                    var partner = c.Tamer?.Partner;
                                    if (partner == null || !partner.Alive) continue;
                                    if (partner.Location.MapId != mapId) continue;
                                    if (UtilitiesFunctions.CalculateDistance(
                                        z.CenterX, partner.Location.X,
                                        z.CenterY, partner.Location.Y) > z.Radius) continue;

                                    var newHp = partner.ReceiveDamage(dmg);
                                    var hpRate = (byte)((long)partner.CurrentHp * 255L / System.Math.Max(1, (int)partner.HP));
                                    bool died = newHp <= 0;
                                    if (died) partner.Die();
                                    hits.Add(new MobAreaSkillPacket.Hit(partner.GeneralHandler, dmg, hpRate, died));
                                }
                                if (hits.Count > 0)
                                    map.BroadcastForTargetTamers(viewing,
                                        new MobAreaSkillPacket(z.CasterHandler, z.SourceSkillIndex,
                                            System.Array.Empty<int>(), hits).Serialize());
                            });
                        AddAoeZone(zone);
                    }
                    break;
                }

                // ─── CONTINUE_WIDE_ATTACK (eEFFECT_TYPE = 20) ──────────────
                // Persistent caster-anchored AoE — damages everything in radius on each
                // tick.  Mob moves → zone follows (re-reads caster position each tick).
                // Bin fields: MaxValue = total duration ms (fallback 5000).  Tick once
                // per 1000 ms.
                case EffectContinueWide:
                {
                    int radius = MobSkillRadius(targetSkill);
                    int lifetimeMs = targetSkill.MaxValue > 0 ? targetSkill.MaxValue : 5_000;
                    int interval = 1_000;
                    int ticks = System.Math.Max(1, lifetimeMs / interval);
                    int dmg = RollMonsterSkillValue(targetSkill);
                    int skillIndex = targetSkill.SkillId;
                    int casterHandler = mob.GeneralHandler;
                    int mapId = mob.Location.MapId;
                    var viewing = mob.TamersViewing;
                    var anchor = mob;

                    BroadcastForTargetTamers(viewing,
                        new MonsterSkillVisualPacket(casterHandler, skillIndex).Serialize());

                    var zone = new MapAoeZone(
                        casterHandler, skillIndex,
                        mob.CurrentLocation.X, mob.CurrentLocation.Y, radius,
                        interval, ticks,
                        (z, map) =>
                        {
                            int cx = anchor.CurrentLocation.X, cy = anchor.CurrentLocation.Y;
                            var hits = new List<MobAreaSkillPacket.Hit>();
                            foreach (var c in map.Clients.ToList())
                            {
                                var partner = c.Tamer?.Partner;
                                if (partner == null || !partner.Alive) continue;
                                if (partner.Location.MapId != mapId) continue;
                                if (UtilitiesFunctions.CalculateDistance(
                                    cx, partner.Location.X, cy, partner.Location.Y) > z.Radius) continue;
                                var newHp = partner.ReceiveDamage(dmg);
                                var hpRate = (byte)((long)partner.CurrentHp * 255L / System.Math.Max(1, (int)partner.HP));
                                bool died = newHp <= 0;
                                if (died) partner.Die();
                                hits.Add(new MobAreaSkillPacket.Hit(partner.GeneralHandler, dmg, hpRate, died));
                            }
                            if (hits.Count > 0)
                                map.BroadcastForTargetTamers(viewing,
                                    new MobAreaSkillPacket(casterHandler, skillIndex,
                                        System.Array.Empty<int>(), hits).Serialize());
                        });
                    AddAoeZone(zone);
                    break;
                }

                // ─── Region_Buff_Nesting (eEFFECT_TYPE = 23) ───────────────
                // Ground-zone (mob-anchored) that ticks a debuff onto every in-zone
                // partner.  No tick damage — purely the debuff carrier.  Doesn't restack
                // the same buff (skip if target already has BuffId).  Bin fields:
                //   EffectFactor[0]      = debuff BuffId (fallback to MinValue)
                //   EffectFactorValue[0] = debuff duration ms (fallback default)
                //   MaxValue             = zone lifetime ms (fallback 10s)
                case EffectRegionBuffNesting:
                {
                    int buffId = targetSkill.EffectFactor.Length > 0 && targetSkill.EffectFactor[0] > 0
                        ? targetSkill.EffectFactor[0]
                        : (int)targetSkill.MinValue;
                    if (buffId <= 0) break;
                    int debuffMs = targetSkill.EffectFactorValue.Length > 0 && targetSkill.EffectFactorValue[0] > 0
                        ? (int)System.Math.Min(targetSkill.EffectFactorValue[0], (uint)int.MaxValue)
                        : DefaultMonsterCastBuffMs;
                    int lifetimeMs = targetSkill.MaxValue > 0 ? targetSkill.MaxValue : 10_000;
                    int interval = 1_000;
                    int ticks = System.Math.Max(1, lifetimeMs / interval);
                    int skillIndex = targetSkill.SkillId;
                    int casterHandler = mob.GeneralHandler;
                    int radius = MobSkillRadius(targetSkill);
                    int mapId = mob.Location.MapId;
                    var viewing = mob.TamersViewing;

                    BroadcastForTargetTamers(viewing,
                        new MonsterSkillVisualPacket(casterHandler, skillIndex).Serialize());

                    var zone = new MapAoeZone(
                        casterHandler, skillIndex,
                        mob.CurrentLocation.X, mob.CurrentLocation.Y, radius,
                        interval, ticks,
                        (z, map) =>
                        {
                            foreach (var c in map.Clients.ToList())
                            {
                                var partner = c.Tamer?.Partner;
                                if (partner == null || !partner.Alive) continue;
                                if (partner.Location.MapId != mapId) continue;
                                if (UtilitiesFunctions.CalculateDistance(
                                    z.CenterX, partner.Location.X,
                                    z.CenterY, partner.Location.Y) > z.Radius) continue;
                                // Don't restack the same debuff every tick.
                                if (partner.DebuffList.Buffs.Any(b => b.BuffId == buffId)) continue;
                                partner.DebuffList.Add(DigimonDebuffModel.Create(buffId, skillIndex, 0, debuffMs));
                                map.BroadcastForTargetTamers(viewing,
                                    new AddBuffPacket(partner.GeneralHandler, buffId, skillIndex, 0, debuffMs).Serialize());
                            }
                        });
                    AddAoeZone(zone);
                    break;
                }

                // ─── RandomAoE (eEFFECT_TYPE = 27) ─────────────────────────
                // Pick N random positions in a ring around the mob, deal damage to any
                // partner inside any picked radius.  One-tick — no persistent zone needed
                // for the damage itself, but the client expects a position-list packet
                // (RecvAroundSkillAni ActiveType=4 path).  Since adding a new packet
                // writer requires a matching client handler revisit, we shipphase 3 with
                // per-target SkillHitPacket (already supported by every client) and
                // accept the cosmetic miss — gameplay-correct, VFX is generic.  Bin:
                //   TargetCount = number of impact points.
                case EffectRandomAoE:
                {
                    int posCount = System.Math.Max(1, (int)targetSkill.TargetCount);
                    int dmg = RollMonsterSkillValue(targetSkill);
                    // RandomAoE per-impact radius — bin-driven.  Spread (placement jitter
                    // around the mob) intentionally stays a server-side aesthetic constant;
                    // it isn't carried by the bin.
                    int radius = targetSkill.RangeUnits > 0 ? (int)targetSkill.RangeUnits : 600;
                    int spread = 1500;
                    int mapId = mob.Location.MapId;

                    BroadcastForTargetTamers(mob.TamersViewing,
                        new MonsterSkillVisualPacket(mob.GeneralHandler, targetSkill.SkillId).Serialize());

                    var rng = System.Random.Shared;
                    var positions = new List<(int x, int y)>(posCount);
                    for (int i = 0; i < posCount; i++)
                        positions.Add((
                            mob.CurrentLocation.X + rng.Next(-spread, spread + 1),
                            mob.CurrentLocation.Y + rng.Next(-spread, spread + 1)));

                    var hits = new List<MobAreaSkillPacket.Hit>();
                    foreach (var c in Clients.ToList())
                    {
                        var partner = c.Tamer?.Partner;
                        if (partner == null || !partner.Alive) continue;
                        if (partner.Location.MapId != mapId) continue;
                        bool inAny = false;
                        foreach (var pos in positions)
                        {
                            if (UtilitiesFunctions.CalculateDistance(
                                pos.x, partner.Location.X, pos.y, partner.Location.Y) <= radius)
                            { inAny = true; break; }
                        }
                        if (!inAny) continue;
                        var newHp = partner.ReceiveDamage(dmg);
                        var hpRate = (byte)((long)partner.CurrentHp * 255L / System.Math.Max(1, (int)partner.HP));
                        bool died = newHp <= 0;
                        if (died) partner.Die();
                        hits.Add(new MobAreaSkillPacket.Hit(partner.GeneralHandler, dmg, hpRate, died));
                    }
                    if (hits.Count > 0)
                        BroadcastForTargetTamers(mob.TamersViewing,
                            new MobAreaSkillPacket(mob.GeneralHandler, targetSkill.SkillId,
                                System.Array.Empty<int>(), hits).Serialize());
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


                                if (diff <= MobSkillRadius(targetSkill))
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

                default:
                {
                    WarnUnhandledSkillTypeOnce(targetSkill.SkillType, targetSkill.SkillId, mob.Type);
                    ApplyFallbackDamageSkill(mob, targetSkill, ResolveFallbackDamage(targetSkill));
                    break;
                }
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


                                if (diff <= MobSkillRadius(targetSkill))
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

                default:
                {
                    WarnUnhandledSkillTypeOnce(targetSkill.SkillType, targetSkill.SkillId, mob.Type);
                    ApplyFallbackDamageSkill(mob, targetSkill, ResolveFallbackDamage(targetSkill));
                    break;
                }
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
            _mobsToAdd.Clear();
            if (MobsToAdd != null)
            {
                _mobsToAdd.AddRange(
                    MobsToAdd
                        .Where(mob => mob != null)
                        .GroupBy(mob => mob.Id)
                        .Select(group => group.First()));
            }

            _mobsToDestroy.Clear();
            if (MobsToRemove != null)
                _mobsToDestroy.AddRange(MobsToRemove);
        }
    }
}
