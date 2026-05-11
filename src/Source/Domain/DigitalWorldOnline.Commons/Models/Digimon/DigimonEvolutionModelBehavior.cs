namespace DigitalWorldOnline.Commons.Models.Digimon
{
    public sealed partial class DigimonEvolutionModel
    {

        /// <summary>
        /// Increases the current skill experience.
        /// <param name="value">Amount to increase</param>
        /// </summary>
        public void IncreaseSkillExperience(int value) => SkillExperience += value;

        /// <summary>
        /// Unlocks the target evolution.
        /// </summary>
        public void Unlock(byte value = 1) => Unlocked = value;

        /// <summary>
        /// Unlocks the target evolution ride mode.
        /// </summary>
        public void UnlockRide() => Unlocked += 8;

        /// <summary>
        /// Increases the skill points amount.
        /// </summary>
        /// <param name="points">Points to increase</param>
        public void IncreaseSkillPoints(byte points = 4)
        {
            SkillPoints += points;
            SkillMastery++;
        }

        /// <summary>
        /// Decrease the skill points amount.
        /// </summary>
        /// <param name="points">Points to increase</param>
        public void DecreaseSkillPoints(byte points)
        {
            // Clamp at 0 — SkillPoints is byte; any underflow would wrap to 255+.
            // Callers gate on `SkillPoints >= cost` but this defends against double-spend
            // / race / future-caller mistakes.
            SkillPoints = points >= SkillPoints ? (byte)0 : (byte)(SkillPoints - points);
        }
        /// <summary>
        /// Inserts new skills into the list.
        /// </summary>
        /// <param name="amount">The amount to add</param>
        public void AddSkill(int amount = 1)
        {
            for (int i = 0; i < amount; i++)
                Skills.Add(new DigimonEvolutionSkillModel());
        }

        public void SetId(long id)
        {
            Id = id;
        }

        /// <summary>
        /// Propagates per-slot skill max-levels from the asset loader (sourced from
        /// <c>DMBase.bin</c> section 12, DigimonEvoMaxLevel) to each skill in this evolution.
        /// Sets only as many skills as caps are provided; extra slots keep their default.
        /// </summary>
        public void SetSkillMaxLevels(IReadOnlyList<byte> caps)
        {
            if (caps == null) return;
            int n = Math.Min(caps.Count, Skills.Count);
            for (int i = 0; i < n; i++)
                Skills[i].SetMaxLevel(caps[i]);
        }
        /// <summary>
        /// Serializes the object into byte array.
        /// </summary>
        public byte[] ToArray()
        {
            using MemoryStream m = new();


            m.Write(BitConverter.GetBytes(GetSkillExpValue(SkillExperience, SkillMastery)), 0, 4);
            m.WriteByte(Unlocked);
            m.WriteByte(0);
            m.WriteByte(0);
            m.WriteByte(0);

            m.WriteByte(SkillPoints);

            // Client cEvoUnit (#pragma pack(1) + SERVER_KSW_SKILL_LV_EXPEND_181205 active for USA)
            // expects exactly nLimit::Skill (=5) bytes for SkillLevel followed by 5 bytes for SkillMaxLevel.
            // Always emit 5+5 regardless of Skills.Count, padding short Skills lists with zero.
            const int kSkillSlots = 5;
            for (int i = 0; i < kSkillSlots; i++)
                m.WriteByte(i < Skills.Count ? Skills[i].CurrentLevel : (byte)0);

            for (int i = 0; i < kSkillSlots; i++)
                m.WriteByte(i < Skills.Count ? Skills[i].MaxLevel : (byte)0);

            return m.ToArray();
        }
        public int GetSkillExpValue(int skillExp, int skillLevel)
        {
            // Calcula o valor inteiro correspondente ao nível
            int m_nSkillExpLevel = skillLevel;

            // Calcula o valor inteiro correspondente à experiência
            int m_nSkillExp = skillExp;

            // Combina os valores para formar o valor inteiro final
            int value = (m_nSkillExpLevel << 26) | m_nSkillExp;

            return value;
        }
    }
}
