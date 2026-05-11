namespace DigitalWorldOnline.Commons.Models.Map
{
    /// <summary>
    /// A mob-spawn request queued by a SUMMON-class mob skill (SUMMON_MONSTER 13 /
    /// SummonPos 31).  The domain layer queues these on the map; the Distribution
    /// layer's <c>MapServerMonsterOperation</c> drains the queue post-tick because
    /// it owns the bin catalog (<c>AssetsLoader.Monster</c>) and the existing
    /// <c>AddSummonMobs</c> broadcast path — both of which the Commons layer can't see.
    /// </summary>
    public sealed class PendingSummon
    {
        public int MonsterTypeId { get; }
        public int X { get; }
        public int Y { get; }
        public int Count { get; }
        public long CasterMobId { get; }
        public int CasterTargetTamerHandler { get; }

        public PendingSummon(int monsterTypeId, int x, int y, int count, long casterMobId, int casterTargetTamerHandler)
        {
            MonsterTypeId = monsterTypeId;
            X = x;
            Y = y;
            Count = count;
            CasterMobId = casterMobId;
            CasterTargetTamerHandler = casterTargetTamerHandler;
        }
    }
}
