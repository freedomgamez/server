using DigitalWorldOnline.Commons.Enums.ClientEnums;
using DigitalWorldOnline.Commons.Enums.Map;
using DigitalWorldOnline.Commons.Models.Character;

namespace DigitalWorldOnline.Commons.Models.Config
{
    public sealed partial class MobConfigModel : StatusAssetModel, ICloneable
    {
        /// <summary>
        /// Sequencial unique identifier.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Client reference for digimon type.
        /// </summary>
        public int Type { get; private set; }

        /// <summary>
        /// Client reference for digimon model.
        /// </summary>
        public int Model { get; private set; }

        /// <summary>
        /// Digimon name.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Base digimon level.
        /// </summary>
        public byte Level { get; private set; }

        /// <summary>
        /// Digimon scaling type.
        /// </summary>
        public byte ScaleType { get; private set; }

        /// <summary>
        /// View range (from current position) for aggressive mobs.
        /// </summary>
        public int ViewRange { get; private set; }

        /// <summary>
        /// Hunt range (from start position) for giveup on chasing targets.
        /// </summary>
        public int HuntRange { get; private set; }

        /// <summary>
        /// Monster class type enumeration. 8 = Raid Boss
        /// </summary>
        public int Class { get; private set; }

        /// <summary>
        /// Monster coliseum Type
        /// </summary>
        public bool Coliseum { get; set; }

        /// <summary>
        /// Monster coliseum Round
        /// </summary>
        public byte Round { get; set; }

        public DungeonDayOfWeekEnum WeekDay { get; set; }

        /// <summary>
        /// Mob Coliseum  type.
        /// </summary>
        public ColiseumMobTypeEnum ColiseumMobType { get; set; }

        /// <summary>
        /// Digimon reaction type.
        /// </summary>
        public DigimonReactionTypeEnum ReactionType { get; private set; }

        /// <summary>
        /// Digimon attribute.
        /// </summary>
        public DigimonAttributeEnum Attribute { get; private set; }

        /// <summary>
        /// Digimon element.
        /// </summary>
        public DigimonElementEnum Element { get; private set; }

        /// <summary>
        /// Digimon main family.
        /// </summary>
        public DigimonFamilyEnum Family1 { get; private set; }

        /// <summary>
        /// Digimon second family.
        /// </summary>
        public DigimonFamilyEnum Family2 { get; private set; }

        /// <summary>
        /// Digimon third family.
        /// </summary>
        public DigimonFamilyEnum Family3 { get; private set; }

        /// <summary>
        /// Respawn interval in seconds.
        /// </summary>
        public int RespawnInterval { get; private set; }

        /// <summary>
        /// Initial location.
        /// </summary>
        public MobLocationConfigModel Location { get; private set; }

        /// <summary>
        /// Drop config.
        /// </summary>
        public MobDropRewardConfigModel? DropReward { get; private set; }

        /// <summary>
        /// Exp config.
        /// </summary>
        public MobExpRewardConfigModel? ExpReward { get; private set; }

        public MobDebuffListModel DebuffList { get; private set; }
        //Dynamic
        public MobActionEnum CurrentAction { get; private set; }
        public DateTime LastActionTime { get; private set; }
        public DateTime LastSkillTryTime { get; private set; }
        public DateTime NextWalkTime { get; private set; }
        public DateTime AgressiveCheckTime { get; private set; }
        public DateTime ViewCheckTime { get; private set; }
        public DateTime LastHitTime { get; private set; }
        public DateTime LastSkillTime { get; private set; }
        public DateTime LastHealTime { get; private set; }
        public DateTime ChaseEndTime { get; private set; }
        public DateTime BattleStartTime { get; private set; } //TODO: utilizar no grow
        public DateTime LastHitTryTime { get; private set; }
        public DateTime LasSkillTryTime { get; private set; }
        public DateTime DieTime { get; private set; }
        public int GeneralHandler { get; private set; }
        public int CurrentHP { get; private set; }
        public int Cooldown { get; private set; }

        // ─── per-skill cooldown + cast window (Monster.bin §3 / Step 6) ──────
        // Replaces the old single-global `Cooldown` — that's still here for any
        // caller that hasn't been migrated, but mob-skill rotation now reads
        // these instead.  Source fields: bin's s_dwCoolTime / s_nCastTime /
        // s_nCastCheck per CsMonsterSkill::sINFO.
        /// <summary>Per-skill cooldown end-times keyed by SkillIndex (bin's s_nSkill_IDX).</summary>
        public Dictionary<int, DateTime> SkillCooldowns { get; } = new();
        /// <summary>Skill index currently being cast, or null when not casting.</summary>
        public int? CastingSkillIndex { get; private set; }
        /// <summary>When the current cast completes — damage applies on the tick AFTER this.</summary>
        public DateTime CastingUntil { get; private set; }
        /// <summary>True when the bin says s_nCastCheck==1 for the active cast — mob root-locked until cast complete.</summary>
        public bool CastingMovementLocked { get; private set; }
        public Location CurrentLocation { get; private set; }
        public Location PreviousLocation { get; private set; }
        public Location InitialLocation { get; private set; }
        public byte MoveCount { get; private set; }
        public byte GrowStack { get; set; }

        // Phase-4 stat-mod state — GROWTH (14) and BERSERK (19) snapshot the base
        // AT/DE values on first apply so the revert path returns to the pre-buff
        // numbers exactly (stacking GROWTH adds on top of the base, not the prior).
        public int BaseATSnapshot { get; private set; }
        public int BaseDESnapshot { get; private set; }
        public DateTime GrowExpiresAt { get; set; }
        public bool Berserk { get; set; }
        public DateTime BerserkExpiresAt { get; set; }
        public int BerserkReflectDamage { get; set; }

        public void EnsureStatSnapshot()
        {
            if (BaseATSnapshot == 0) BaseATSnapshot = ATValue;
            if (BaseDESnapshot == 0) BaseDESnapshot = DEValue;
        }

        public void RestoreBaseStats()
        {
            if (BaseATSnapshot > 0) SetAT(BaseATSnapshot);
            if (BaseDESnapshot > 0) SetDE(BaseDESnapshot);
        }

        public byte DisposedObjects { get; set; }
        public bool InBattle { get; private set; }
        public bool AwaitingKillSpawn { get; private set; }
        public bool Dead { get; private set; }
        public bool Respawn { get; private set; }
        public bool CheckSkill { get; private set; }
        public bool IsPossibleSkill => ((double)CurrentHP / HPValue) * 100 <= 90;
        public bool Bless => Type == 45161 && Location.MapId == 1307;
        public bool BossMonster { get; private set; } //TODO: ajustar valor conforme type
        public List<CharacterModel> TargetTamers { get; private set; }
        public Dictionary<long, int> RaidDamage { get; private set; }
        public List<long> TamersViewing { get; private set; }

        public MobConfigModel()
        {
            TamersViewing = new List<long>();
            RaidDamage = new Dictionary<long, int>();
            TargetTamers = new List<CharacterModel>();
            Location = new MobLocationConfigModel();
            RespawnInterval = 8;
            DebuffList = new MobDebuffListModel();
            CurrentAction = MobActionEnum.Wait;
            LastActionTime = DateTime.Now;
            AgressiveCheckTime = DateTime.Now;
            ViewCheckTime = DateTime.Now;
            NextWalkTime = DateTime.Now;
            ChaseEndTime = DateTime.Now;
            LastHealTime = DateTime.Now;
            LastHitTime = DateTime.Now;
            LastHitTryTime = DateTime.Now;
            LastSkillTryTime = DateTime.Now;
            DieTime = DateTime.Now;
        }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}
