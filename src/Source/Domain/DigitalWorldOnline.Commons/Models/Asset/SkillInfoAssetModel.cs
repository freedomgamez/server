using DigitalWorldOnline.Commons.DTOs.Base;
using DigitalWorldOnline.Commons.Models.Base;

namespace DigitalWorldOnline.Commons.Models.Asset
{
    public sealed class SkillInfoAssetModel
    {
        /// <summary>
        /// Unique sequential identifier
        /// </summary>
        public long Id { get; private set; }

        /// <summary>
        /// Skill id
        /// </summary>
        public int SkillId { get; private set; }
        
        /// <summary>
        /// Skill name
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Skill Digimon Family Type
        /// </summary>
        public byte FamilyType { get; private set; }

        /// <summary>
        /// DS usage
        /// </summary>
        public int DSUsage { get; private set; }
        
        /// <summary>
        /// HP usage
        /// </summary>
        public int HPUsage { get; private set; }
        
        /// <summary>
        /// Damage or Healling value
        /// </summary>
        public int Value { get; private set; }
        
        /// <summary>
        /// Total casting time
        /// </summary>
        public float CastingTime { get; private set; }
        
        /// <summary>
        /// Cooldown in seconds
        /// </summary>
        public int Cooldown { get; private set; }
        
        /// <summary>
        /// Max skill upgrade level
        /// </summary>
        public byte MaxLevel { get; private set; }
        
        /// <summary>
        /// Required skill points to level up the skill
        /// </summary>
        public byte RequiredPoints { get; private set; }
        
        /// <list type="number">
        ///     <listheader>
        ///         <term>Skill target type</term>
        ///         <description>The type of the skill target</description>
        ///     </listheader>
        ///     <item>
        ///         <term>51</term>
        ///         <description>Single target and single shot damage</description>
        ///     </item>
        ///     <item>
        ///         <term>21</term>
        ///         <description>Discover...</description>
        ///     </item>
        /// </list>
        public byte Target { get; private set; }

        /// <summary>
        /// The skill area of effect (AoE). Zero means not AoE skill.
        /// </summary>
        public int AreaOfEffect { get; private set; }
        
        /// <summary>
        /// Minimal damage of an Area of Effect skill.
        /// </summary>
        public int AoEMinDamage { get; private set; }
        
        /// <summary>
        /// Maximum damage of an Area of Effect skill.
        /// </summary>
        public int AoEMaxDamage { get; private set; }
        
        /// <summary>
        /// Range to start casting.
        /// </summary>
        public int Range { get; private set; }

        /// <summary>
        /// Level for unlocking the skill.
        /// </summary>
        public byte UnlockLevel { get; private set; }

        /// <summary>
        /// Memory chips used if this is a memory skill.
        /// </summary>
        public byte MemoryChips { get; private set; }
        
        /// <summary>
        /// Buff or debuff inflicted by the skill.
        /// </summary>
        public int FirstConditionCode { get; private set; }
        
        /// <summary>
        /// Buff or debuff inflicted by the skill.
        /// </summary>
        public int SecondConditionCode { get; private set; }
        
        /// <summary>
        /// Buff or debuff inflicted by the skill.
        /// </summary>
        public int ThirdConditionCode { get; private set; }
        
        /// <summary>
        /// The explicit type of the casted skill.
        /// </summary>
        public int Type { get; private set; }

        /// <summary>
        /// CsSkill::sINFO::s_nAttType — see <see cref="IsActive"/> / <see cref="IsPassive"/>.
        /// </summary>
        public int AttType { get; private set; }

        /// <summary>
        /// True for skills the client treats as active (castable from the hotbar).  Mirrors
        /// v487 client's <c>CsSkill::IsActive()</c> in <c>LibProj/CsFileTable/Skill.h:88</c>.
        /// </summary>
        public bool IsActive => AttType != 0 && AttType != 4;

        /// <summary>
        /// True for passive (always-on) skills.  Mirrors <c>CsSkill::IsPasive()</c> at
        /// <c>Skill.h:89</c>.  Server must reject incoming cast packets for passive skills —
        /// passive effects fire from buff state, never from a player-triggered packet.
        /// </summary>
        public bool IsPassive => AttType == 4;

        /// <summary>
        /// CsSkill::sINFO::s_nMemorySkill — 0 = inherent, 1-3 = memory skill rank tier.
        /// </summary>
        public byte MemorySkill { get; private set; }

        /// <summary>
        /// True if this skill is a deletable memory skill (cash-shop acquired).  Inherent
        /// per-evolution skills (slot 0..3 from Digimon_List.bin) have <c>MemorySkill == 0</c>.
        /// </summary>
        public bool IsMemorySkill => MemorySkill > 0;

        /// <summary>
        /// CsSkill::sINFO::s_nSkillGroup — overlap key.  A digimon can only equip one
        /// memory skill per group at a time.
        /// </summary>
        public int SkillGroup { get; private set; }

        /// <summary>
        /// CsSkill::sINFO::s_nSkillRank — 1=low, 2=mid, 3=high (memory skills only).
        /// </summary>
        public int SkillRank { get; private set; }

        /// <summary>
        /// The description about the skill.
        /// </summary>
        public string Description { get; private set; }

    }
}