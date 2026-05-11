namespace DigitalWorldOnline.Commons.DTOs.Assets
{
    /// <summary>
    /// One row of <c>Skill.bin §3 CsAreaCheck::sArea_Check</c>.  Drives the area-restriction
    /// check for tamer skills flagged with <c>UseAreaCheck = 1</c>.  <see cref="Index"/>
    /// matches <c>TamerSkillAssetDTO.SkillId</c>; <see cref="MapIds"/> is up to 30 MapId
    /// entries (zeros = unused slots, dropped at load) interpreted per the parent skill's
    /// <c>Available</c> flag (1 = blacklist, 2 = whitelist).
    /// </summary>
    public sealed class TamerSkillAreaCheckAssetDTO
    {
        public int Index { get; set; }
        public int[] MapIds { get; set; } = System.Array.Empty<int>();
    }
}
