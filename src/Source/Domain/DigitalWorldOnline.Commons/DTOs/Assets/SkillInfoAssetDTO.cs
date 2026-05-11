using DigitalWorldOnline.Commons.DTOs.Base;

namespace DigitalWorldOnline.Commons.DTOs.Assets
{
    public sealed class SkillInfoAssetDTO
    {
        /// <summary>
        /// Unique sequential identifier
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Skill id
        /// </summary>
        public int SkillId { get; set; }
        
        /// <summary>
        /// Skill name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Skill Digimon Family Type
        /// </summary>
        public byte FamilyType { get; set; }

        /// <summary>
        /// DS usage
        /// </summary>
        public int DSUsage { get; set; }
        
        /// <summary>
        /// HP usage
        /// </summary>
        public int HPUsage { get; set; }
        
        /// <summary>
        /// Damage or Healling value
        /// </summary>
        public int Value { get; set; }
        
        /// <summary>
        /// Total casting time
        /// </summary>
        public float CastingTime { get; set; }
        
        /// <summary>
        /// Cooldown in seconds
        /// </summary>
        public int Cooldown { get; set; }
        
        /// <summary>
        /// Max skill upgrade level
        /// </summary>
        public byte MaxLevel { get; set; }
        
        /// <summary>
        /// Required skill points to level up the skill
        /// </summary>
        public byte RequiredPoints { get; set; }
        
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
        public byte Target { get; set; }

        /// <summary>
        /// The skill area of effect (AoE). Zero means not AoE skill.
        /// </summary>
        public int AreaOfEffect { get; set; }
        
        /// <summary>
        /// Minimal damage of an Area of Effect skill.
        /// </summary>
        public int AoEMinDamage { get; set; }
        
        /// <summary>
        /// Maximum damage of an Area of Effect skill.
        /// </summary>
        public int AoEMaxDamage { get; set; }
        
        /// <summary>
        /// Range to start casting.
        /// </summary>
        public int Range { get; set; }
        
        /// <summary>
        /// Level for unlocking the skill.
        /// </summary>
        public byte UnlockLevel { get; set; }
        
        /// <summary>
        /// Memory chips used if this is a memory skill.
        /// </summary>
        public byte MemoryChips { get; set; }
        
        /// <summary>
        /// Buff or debuff inflicted by the skill.
        /// </summary>
        public int FirstConditionCode { get; set; }
        
        /// <summary>
        /// Buff or debuff inflicted by the skill.
        /// </summary>
        public int SecondConditionCode { get; set; }
        
        /// <summary>
        /// Buff or debuff inflicted by the skill.
        /// </summary>
        public int ThirdConditionCode { get; set; }
        
        /// <summary>
        /// The explicit type of the casted skill.
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// CsSkill::sINFO::s_nAttType — drives <c>IsActive()</c>/<c>IsPasive()</c> on the
        /// client (<c>Skill.h:88-89</c>). Active skills are usable from the hotbar
        /// (<c>AttType != 0 &amp;&amp; AttType != 4</c>); passive skills (<c>AttType == 4</c>)
        /// are always-on buffs and must NOT be cast as attack packets.
        /// </summary>
        public int AttType { get; set; }

        /// <summary>
        /// CsSkill::sINFO::s_nMemorySkill — 0 = inherent skill (cannot be deleted),
        /// 1-3 = memory skill (cash-shop, deletable; ranks low/mid/high).  See
        /// <c>Skill.h:76</c> Korean comment.
        /// </summary>
        public byte MemorySkill { get; set; }

        /// <summary>
        /// CsSkill::sINFO::s_nSkillGroup — used by client to detect overlap between
        /// memory skills (you can't equip two memory skills of the same group on the same
        /// digimon).  Server enforces the same on grant.
        /// </summary>
        public int SkillGroup { get; set; }

        /// <summary>
        /// CsSkill::sINFO::s_nSkillRank — for memory skills only: 1 = low, 2 = mid,
        /// 3 = high (matching the rank tiers in <see cref="MemorySkill"/>).
        /// </summary>
        public int SkillRank { get; set; }

        /// <summary>
        /// The description about the skill.
        /// </summary>
        public string Description { get; set; }
    }
}