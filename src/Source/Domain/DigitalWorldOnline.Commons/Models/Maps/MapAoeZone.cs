using System;

namespace DigitalWorldOnline.Commons.Models.Map
{
    /// <summary>
    /// A persistent area-of-effect zone on a <see cref="MapInstance"/>.  Used by mob skills
    /// whose effect lingers in space + time: ATTACH_SEED (18), Region_Buff_Nesting (23),
    /// RandomAoE (27 — degenerate one-tick), CONTINUE_WIDE_ATTACK (20 — anchored to caster).
    ///
    /// The map's tick loop calls <see cref="Tick"/> on every cycle; when
    /// <see cref="ExpiresAt"/> is reached the zone is removed.  Per-tick damage / buff
    /// application lives in <see cref="OnTick"/> (closure over the spawning skill data).
    /// </summary>
    public sealed class MapAoeZone
    {
        /// <summary>Caster mob handler — used only for the source-handler field on hit packets.</summary>
        public int CasterHandler { get; }
        public int SourceSkillIndex { get; }
        public int CenterX { get; }
        public int CenterY { get; }
        public int Radius { get; }
        public DateTime ExpiresAt { get; }
        public int TickIntervalMs { get; }
        public DateTime NextTick { get; private set; }
        public int TicksRemaining { get; private set; }
        public Action<MapAoeZone, MapInstance> OnTick { get; }

        public MapAoeZone(
            int casterHandler, int skillIndex,
            int centerX, int centerY, int radius,
            int tickIntervalMs, int ticks,
            Action<MapAoeZone, MapInstance> onTick)
        {
            CasterHandler = casterHandler;
            SourceSkillIndex = skillIndex;
            CenterX = centerX;
            CenterY = centerY;
            Radius = radius;
            TickIntervalMs = tickIntervalMs;
            TicksRemaining = Math.Max(0, ticks);
            ExpiresAt = DateTime.Now.AddMilliseconds((long)tickIntervalMs * Math.Max(1, ticks));
            NextTick = DateTime.Now.AddMilliseconds(tickIntervalMs);
            OnTick = onTick;
        }

        /// <summary>Returns true if this zone should be removed after this call.</summary>
        public bool Tick(MapInstance map)
        {
            var now = DateTime.Now;
            while (TicksRemaining > 0 && now >= NextTick)
            {
                OnTick(this, map);
                TicksRemaining--;
                NextTick = NextTick.AddMilliseconds(TickIntervalMs);
            }
            return TicksRemaining <= 0 || now >= ExpiresAt;
        }
    }
}
