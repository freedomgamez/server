namespace DigitalWorldOnline.Application.GameAssets.Bins;

public sealed record MapBin(
    IReadOnlyDictionary<int, MapListRecord> MapsById,
    IReadOnlyDictionary<int, IReadOnlyList<MapPortalRecord>> PortalsBySourceMapId,
    IReadOnlyDictionary<int, IReadOnlyList<MapRegionRecord>> RegionsByMapId,
    IReadOnlyDictionary<int, IReadOnlyList<MapLimitEvoRecord>> LimitEvoByMapId,
    IReadOnlyDictionary<int, IReadOnlyList<MapBuffRegionRecord>> BuffRegionsByMapId,
    IReadOnlyDictionary<int, IReadOnlyList<MapMonsterRecord>> MonstersByMapId);

public sealed record MapListRecord(
    int MapId,
    int Width,
    int Height,
    int ResurrectionMapId,
    ushort MapRegionId);

public sealed record MapPortalRecord(
    int PortalId,
    int PortalType,
    int SourceMapId,
    int DestMapId,
    int SourceX,
    int SourceY,
    int SourceRadius,
    int DestX,
    int DestY,
    int DestRadius,
    int PortalObjectType,
    int UniqObjectId,
    int PortalKindIndex);

public sealed record MapRegionRecord(
    int MapId,
    int CenterX,
    int CenterY,
    int Radius,
    ushort FatigueType,
    ushort FatigueDebuff,
    ushort FatigueStartTime,
    ushort FatigueAddTime,
    short FatigueAddPoint);

public sealed record MapLimitEvoRecord(
    int MapId,
    int CenterX,
    int CenterY,
    int Radius);

public sealed record MapBuffRegionRecord(
    int MapId,
    int RegionIndex,
    int CenterX,
    int CenterY,
    int Radius,
    int Act,
    int BuffId,
    int Time,
    int LayerNo,
    int ApplyType,
    int Requirement1,
    int Requirement2,
    int ObjectUniqueId);

public sealed record MapMonsterRecord(
    int MapId,
    int MonsterTableId,
    int CenterX,
    int CenterY,
    int Radius,
    int Count,
    int RespawnSeconds);
