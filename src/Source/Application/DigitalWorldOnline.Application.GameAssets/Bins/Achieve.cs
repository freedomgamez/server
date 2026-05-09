namespace DigitalWorldOnline.Application.GameAssets.Bins;

/// <summary>
/// In-memory <c>Achieve.bin</c> contents. The category-tree section (sTYPE recursive
/// label nodes — pure UI strings) is skipped entirely on load; only the achievement
/// detail records are kept.
/// </summary>
public sealed class Achieve
{
    public IReadOnlyList<AchieveRecord> All { get; }
    public IReadOnlyDictionary<int, AchieveRecord> ByQuestId { get; }

    public Achieve(IReadOnlyList<AchieveRecord> all)
    {
        All = all;
        ByQuestId = all.ToDictionary(a => a.QuestId);
    }
}

/// <summary>
/// One <c>CsAchieve::sINFO</c> entry from <c>AchieveMng.h</c>. v487 layout (796 bytes).
/// String fields (<c>s_szName</c>, <c>s_szComment</c>, <c>s_szTitle</c>) are skipped.
/// <para>
/// <see cref="QuestId"/> is the join key against <c>Quest.bin</c> — when a quest
/// completes, the achievement of the same id is considered earned.
/// <see cref="BuffCode"/> is non-zero only for achievements that grant a title; the
/// title's effect is exclusively that buff (v487 has no per-title flat-stat block —
/// see <c>SetTitlePacketProcessor.cs:64-91</c>).
/// </para>
/// <para>
/// <see cref="Icon"/>, <see cref="Point"/>, <see cref="DisplayHidden"/>,
/// <see cref="Group"/>, <see cref="SubGroup"/>, <see cref="Type"/> are loaded for
/// parity but currently have no server consumer (UI-driven on the client).
/// </para>
/// </summary>
public sealed record AchieveRecord(
    int QuestId,
    uint Icon,
    ushort Point,
    bool DisplayHidden,
    int Group,
    int SubGroup,
    int Type,
    int BuffCode);
