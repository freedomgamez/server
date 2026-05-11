namespace DigitalWorldOnline.Application.GameAssets.Bins;

/// <summary>
/// In-memory <c>Buff.bin</c> contents — single section of <c>CsBuff::sINFO</c> records
/// (see <c>LibProj/CsFileTable/Buff.h</c>). String fields are skipped per
/// <c>reference_bin_string_framing.md</c>.
/// </summary>
public sealed class Buff
{
    public IReadOnlyDictionary<int, BuffRecord> ById { get; }

    public Buff(IReadOnlyDictionary<int, BuffRecord> byId)
    {
        ById = byId;
    }

    public BuffRecord? Find(int id) => ById.TryGetValue(id, out var rec) ? rec : null;
}

/// <summary>
/// One <c>CsBuff::sINFO</c> entry. v487 layout (476 bytes on disk).
/// <para>
/// <see cref="MinLevel"/> + <see cref="BuffClass"/> drive the client-side overlap rule
/// (`DataMng.cpp:3895-3897`): when adding a buff, reject the incoming if an existing buff
/// with the same <c>BuffClass</c> has <c>MinLevel</c> &gt;= the incoming. Server should
/// mirror.
/// </para>
/// <para>
/// <see cref="ConditionLevel"/> caps applied skill-level at READ time for condition-buffs
/// (skill apply ID 107 — see `Buff.cpp:260-262`). Stored as-is; downstream consumers clamp.
/// </para>
/// </summary>
public sealed record BuffRecord(
    ushort Id,
    ushort BuffType,
    ushort LifeType,
    ushort TimeType,
    ushort MinLevel,
    ushort BuffClass,
    uint SkillCode,
    uint DigimonSkillCode,
    ushort ConditionLevel,
    bool IsDeleted);
