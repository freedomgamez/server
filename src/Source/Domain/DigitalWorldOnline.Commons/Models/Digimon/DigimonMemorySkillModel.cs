namespace DigitalWorldOnline.Commons.Models.Digimon
{
    public sealed class DigimonMemorySkillModel
    {
        public long Id { get; private set; }
        public int SkillId { get; private set; }
        public byte CurrentLevel { get; private set; }
        public byte MaxLevel { get; private set; }
        public DateTime AcquiredAt { get; private set; }
        public DateTime CooldownEndsAt { get; private set; }

        public bool IsOnCooldown => CooldownEndsAt > DateTime.UtcNow;

        // Default ctor for AutoMapper / EF.
        public DigimonMemorySkillModel() { }

        public static DigimonMemorySkillModel Create(int skillId, byte maxLevel) => new()
        {
            SkillId = skillId,
            CurrentLevel = 1,
            MaxLevel = maxLevel,
            AcquiredAt = DateTime.UtcNow
        };

        public void SetCurrentLevel(byte level) => CurrentLevel = level;
        public void SetMaxLevel(byte level) => MaxLevel = level;
        public void StartCooldown(int cooldownMs) => CooldownEndsAt = DateTime.UtcNow.AddMilliseconds(cooldownMs);
    }
}
