namespace DigitalWorldOnline.Application.GameAssets.Bins;

public static class MapMonsterSpawnMaterializer
{
    private static readonly double GoldenAngle = Math.PI * (3d - Math.Sqrt(5d));

    public sealed record SpawnPoint(
        int MonsterTableId,
        int X,
        int Y,
        int RespawnSeconds);

    public static IEnumerable<SpawnPoint> Expand(MapMonsterRecord record)
    {
        int count = record.Count;
        if (count <= 0)
            yield break;

        if (count == 1 || record.Radius <= 0)
        {
            yield return new SpawnPoint(
                record.MonsterTableId,
                record.CenterX,
                record.CenterY,
                record.RespawnSeconds);
            yield break;
        }

        // Deterministic radial spread (golden-angle spiral) so multi-count spawns
        // are stable across channels/restarts while still using bin Count+Radius.
        double radius = Math.Max(1, record.Radius);
        for (int index = 0; index < count; index++)
        {
            double ratio = Math.Sqrt((index + 0.5d) / count);
            double distance = radius * ratio;
            double angle = index * GoldenAngle;
            int x = record.CenterX + (int)Math.Round(Math.Cos(angle) * distance);
            int y = record.CenterY + (int)Math.Round(Math.Sin(angle) * distance);
            yield return new SpawnPoint(record.MonsterTableId, x, y, record.RespawnSeconds);
        }
    }
}
