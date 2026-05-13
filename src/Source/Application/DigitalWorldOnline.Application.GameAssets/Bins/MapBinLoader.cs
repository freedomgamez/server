namespace DigitalWorldOnline.Application.GameAssets.Bins;

public sealed class MapBinLoader
{
    private const int MapRegionDescriptionLength = 128;
    private const int MapRegionSoundFilenameLength = 64;
    private const int Utf16CharSize = 2;

    private MapBin? _data;

    public MapBin Data => _data ?? throw new InvalidOperationException(
        $"{nameof(MapBinLoader)}: bins not loaded yet — call Load() first.");

    public bool IsLoaded => _data != null;

    public MapBin Load()
    {
        if (_data != null) return _data;

        var dir = BinPath.ResolveDirectory();
        var mapsById = LoadMapList(Path.Combine(dir, "MapList.bin"));
        var portalsByMap = LoadMapPortal(Path.Combine(dir, "MapPortal.bin"));
        var (regionsByMap, limitEvoByMap, buffByMap) = LoadMapRegion(Path.Combine(dir, "MapRegion.bin"));
        var monstersByMap = LoadMapMonsterList(Path.Combine(dir, "MapMonsterList.bin"));

        _data = new MapBin(
            mapsById,
            portalsByMap,
            regionsByMap,
            limitEvoByMap,
            buffByMap,
            monstersByMap);

        return _data;
    }

    private static Dictionary<int, MapListRecord> LoadMapList(string path)
    {
        using var fs = File.OpenRead(path);
        using var r = new BinaryReader(fs);
        int count = r.ReadInt32();
        var result = new Dictionary<int, MapListRecord>(count);
        for (int i = 0; i < count; i++)
        {
            int mapId = r.ReadInt32();
            SkipFramedString(r);
            SkipFramedString(r);
            SkipFramedString(r);
            int width = r.ReadInt32();
            int height = r.ReadInt32();
            SkipFramedWString(r);
            SkipFramedWString(r);
            int resurrectionMapId = r.ReadInt32();
            ushort mapRegionId = r.ReadUInt16();
            _ = r.ReadUInt16(); // fatigue type
            _ = r.ReadUInt16(); // fatigue debuff
            _ = r.ReadUInt16(); // fatigue start time
            _ = r.ReadUInt16(); // fatigue add time
            _ = r.ReadInt16();  // fatigue add point
            _ = r.ReadInt16();  // camera max level
            _ = r.ReadBoolean();
            _ = r.ReadBoolean();

            if (!result.ContainsKey(mapId))
            {
                result.Add(mapId, new MapListRecord(
                    mapId, width, height, resurrectionMapId, mapRegionId));
            }
        }

        return result;
    }

    private static Dictionary<int, IReadOnlyList<MapPortalRecord>> LoadMapPortal(string path)
    {
        using var fs = File.OpenRead(path);
        using var r = new BinaryReader(fs);
        int groupCount = r.ReadInt32();
        var result = new Dictionary<int, List<MapPortalRecord>>();

        for (int i = 0; i < groupCount; i++)
        {
            int portalCount = r.ReadInt32();
            for (int p = 0; p < portalCount; p++)
            {
                int portalId = r.ReadInt32();
                int portalType = r.ReadInt32();
                int sourceMapId = r.ReadInt32();
                int sourceX = r.ReadInt32();
                int sourceY = r.ReadInt32();
                int sourceRadius = r.ReadInt32();
                int destMapId = r.ReadInt32();
                int destX = r.ReadInt32();
                int destY = r.ReadInt32();
                int destRadius = r.ReadInt32();
                int portalObjectType = r.ReadInt32();
                int uniqObjectId = r.ReadInt32();
                int portalKindIndex = r.ReadInt32();
                int viewX = r.ReadInt32();
                int viewY = r.ReadInt32();

                if (!result.TryGetValue(sourceMapId, out var list))
                {
                    list = new List<MapPortalRecord>();
                    result.Add(sourceMapId, list);
                }
                list.Add(new MapPortalRecord(
                    portalId,
                    portalType,
                    sourceMapId,
                    destMapId,
                    sourceX,
                    sourceY,
                    sourceRadius,
                    destX,
                    destY,
                    destRadius,
                    portalObjectType,
                    uniqObjectId,
                    portalKindIndex));
                _ = viewX;
                _ = viewY;
            }
        }

        return result.ToDictionary(kv => kv.Key, kv => (IReadOnlyList<MapPortalRecord>)kv.Value);
    }

    private static (
        Dictionary<int, IReadOnlyList<MapRegionRecord>> Regions,
        Dictionary<int, IReadOnlyList<MapLimitEvoRecord>> LimitEvo,
        Dictionary<int, IReadOnlyList<MapBuffRegionRecord>> BuffRegions) LoadMapRegion(string path)
    {
        using var fs = File.OpenRead(path);
        using var r = new BinaryReader(fs);

        var regions = new Dictionary<int, List<MapRegionRecord>>();
        int groups = r.ReadInt32();
        for (int i = 0; i < groups; i++)
        {
            int cnt = r.ReadInt32();
            for (int j = 0; j < cnt; j++)
            {
                int mapId = r.ReadInt32();
                int centerX = r.ReadInt32();
                int centerY = r.ReadInt32();
                int radius = r.ReadInt32();
                _ = r.ReadBytes(MapRegionDescriptionLength * Utf16CharSize);
                _ = r.ReadBytes(MapRegionDescriptionLength * Utf16CharSize);
                _ = r.ReadBytes(MapRegionSoundFilenameLength);
                ushort fatigueType = r.ReadUInt16();
                ushort fatigueDebuff = r.ReadUInt16();
                ushort fatigueStartTime = r.ReadUInt16();
                ushort fatigueAddTime = r.ReadUInt16();
                short fatigueAddPoint = r.ReadInt16();
                _ = r.ReadInt16(); // MSVC struct tail padding (sizeof(CsMapRegion::sINFO) == 604)

                if (!regions.TryGetValue(mapId, out var list))
                {
                    list = new List<MapRegionRecord>();
                    regions.Add(mapId, list);
                }
                list.Add(new MapRegionRecord(
                    mapId,
                    centerX,
                    centerY,
                    radius,
                    fatigueType,
                    fatigueDebuff,
                    fatigueStartTime,
                    fatigueAddTime,
                    fatigueAddPoint));
            }
        }

        var limit = new Dictionary<int, List<MapLimitEvoRecord>>();
        groups = r.ReadInt32();
        for (int i = 0; i < groups; i++)
        {
            int cnt = r.ReadInt32();
            for (int j = 0; j < cnt; j++)
            {
                int mapId = r.ReadInt32();
                int centerX = r.ReadInt32();
                int centerY = r.ReadInt32();
                int radius = r.ReadInt32();
                if (!limit.TryGetValue(mapId, out var list))
                {
                    list = new List<MapLimitEvoRecord>();
                    limit.Add(mapId, list);
                }
                list.Add(new MapLimitEvoRecord(mapId, centerX, centerY, radius));
            }
        }

        var buff = new Dictionary<int, List<MapBuffRegionRecord>>();
        groups = r.ReadInt32();
        for (int i = 0; i < groups; i++)
        {
            int cnt = r.ReadInt32();
            for (int j = 0; j < cnt; j++)
            {
                int regionIndex = r.ReadInt32();
                int mapId = r.ReadInt32();
                int centerX = r.ReadInt32();
                int centerY = r.ReadInt32();
                int radius = r.ReadInt32();
                int act = r.ReadInt32();
                int buffId = r.ReadInt32();
                int time = r.ReadInt32();
                int layerNo = r.ReadInt32();
                int applyType = r.ReadInt32();
                int requirement1 = r.ReadInt32();
                int requirement2 = r.ReadInt32();
                int objectUniqueId = r.ReadInt32();

                if (!buff.TryGetValue(mapId, out var list))
                {
                    list = new List<MapBuffRegionRecord>();
                    buff.Add(mapId, list);
                }
                list.Add(new MapBuffRegionRecord(
                    mapId,
                    regionIndex,
                    centerX,
                    centerY,
                    radius,
                    act,
                    buffId,
                    time,
                    layerNo,
                    applyType,
                    requirement1,
                    requirement2,
                    objectUniqueId));
            }
        }

        return (
            regions.ToDictionary(kv => kv.Key, kv => (IReadOnlyList<MapRegionRecord>)kv.Value),
            limit.ToDictionary(kv => kv.Key, kv => (IReadOnlyList<MapLimitEvoRecord>)kv.Value),
            buff.ToDictionary(kv => kv.Key, kv => (IReadOnlyList<MapBuffRegionRecord>)kv.Value));
    }

    private static Dictionary<int, IReadOnlyList<MapMonsterRecord>> LoadMapMonsterList(string path)
    {
        using var fs = File.OpenRead(path);
        using var r = new BinaryReader(fs);
        int fileTableCount = r.ReadInt32();
        var result = new Dictionary<int, List<MapMonsterRecord>>();

        for (int i = 0; i < fileTableCount; i++)
        {
            _ = r.ReadUInt32();
            int mapBucketCount = r.ReadInt32();
            for (int j = 0; j < mapBucketCount; j++)
            {
                int mapId = (int)r.ReadUInt32();
                int monsterCount = r.ReadInt32();
                for (int k = 0; k < monsterCount; k++)
                {
                    int rowMapId = (int)r.ReadUInt32();
                    int monsterTableId = (int)r.ReadUInt32();
                    int centerX = r.ReadInt32();
                    int centerY = r.ReadInt32();
                    int radius = r.ReadInt32();
                    int count = r.ReadInt32();
                    int respawn = r.ReadInt32();
                    _ = r.ReadInt32();
                    _ = r.ReadInt32();
                    _ = r.ReadInt32();
                    _ = r.ReadInt32();
                    _ = r.ReadBoolean();
                    _ = r.ReadBytes(3);

                    if (!result.TryGetValue(mapId, out var list))
                    {
                        list = new List<MapMonsterRecord>();
                        result.Add(mapId, list);
                    }
                    list.Add(new MapMonsterRecord(rowMapId, monsterTableId, centerX, centerY, radius, count, respawn));
                }
            }
        }

        return result.ToDictionary(kv => kv.Key, kv => (IReadOnlyList<MapMonsterRecord>)kv.Value);
    }

    private static void SkipFramedString(BinaryReader r)
    {
        int size = r.ReadInt32();
        if (size > 0) _ = r.ReadBytes(size);
    }

    private static void SkipFramedWString(BinaryReader r)
    {
        int chars = r.ReadInt32();
        if (chars > 0) _ = r.ReadBytes(chars * 2);
    }

}
