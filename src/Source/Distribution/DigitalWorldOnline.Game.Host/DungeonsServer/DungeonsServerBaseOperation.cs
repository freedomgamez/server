using DigitalWorldOnline.Application.Separar.Queries;
using DigitalWorldOnline.Application.GameAssets.Queries;
using DigitalWorldOnline.Commons.Entities;
using DigitalWorldOnline.Commons.Enums;
using DigitalWorldOnline.Commons.Enums.ClientEnums;
using DigitalWorldOnline.Commons.Models;
using DigitalWorldOnline.Commons.Models.Character;
using DigitalWorldOnline.Commons.Models.Config;
using DigitalWorldOnline.Commons.Models.Map;
using DigitalWorldOnline.Commons.Models.Mechanics;
using DigitalWorldOnline.Commons.Models.Summon;
using DigitalWorldOnline.Commons.Models.TamerShop;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Threading;

namespace DigitalWorldOnline.GameHost
{
    public sealed partial class DungeonsServer
    {
        private DateTime _lastMapsSearch = DateTime.Now;
        private DateTime _lastMobsSearch = DateTime.Now;
        private DateTime _lastConsignedShopsSearch = DateTime.Now;

        //TODO: externalizar
        private readonly int _startToSee = 6000;
        private readonly int _stopSeeing = 6001;

        /// <summary>
        /// Cleans unused running maps.
        /// </summary>
        public Task CleanMaps()
        {
            _driver.CleanIdle(_registry, _logger);
            return Task.CompletedTask;
        }
        /// <summary>
        /// O(1) dungeon-instance lookup by <see cref="MapInstance.DungeonId"/>.
        /// Use this from packet processors instead of <c>Maps.FirstOrDefault(...)</c>.
        /// </summary>
        public MapInstance? FindMapByDungeonId(long dungeonId) => _registry.FindByDungeonId(dungeonId);

        /// <summary>O(1) tamer→map lookup (cross-type, finds the tamer in any *Server).</summary>
        public MapInstance? FindMapByTamer(long tamerId) => _registry.FindByTamer(tamerId);

        public Task CleanMap(int DungeonId)
        {
            var mapToClose = _registry.FindByDungeonId(DungeonId);

            if(mapToClose != null)
            {
                _registry.Unregister(MapTypeEnum.Dungeon, mapToClose);
            }
            return Task.CompletedTask;
        }
        /// <summary>
        /// Periodic dungeon lifecycle: delegated to <see cref="DungeonMapDriver"/>.
        /// Dungeons are spawned on-demand via <see cref="SearchNewMaps(bool, GameClient)"/>
        /// when a party enters one — the driver's periodic hook is intentionally
        /// a no-op (the pre-rework periodic loop was broken: `!Maps.Any(x =>
        /// x.Id != partymap.Id)` is always true for non-empty lists with
        /// non-matching ids, which spawned an extra dungeon every 10 s).
        /// </summary>
        public async Task SearchNewMaps(CancellationToken cancellationToken)
        {
            if (DateTime.Now > _lastMapsSearch)
            {
                await _driver.RefreshInstances(_sender, _mapper, _registry, _logger, cancellationToken);
                _lastMapsSearch = DateTime.Now.AddSeconds(10);
            }
        }

        public async Task SearchNewMaps(bool IsParty, GameClient client)
        {
            var dtos = await _sender.Send(new GameMapsConfigQuery(MapTypeEnum.Dungeon));

            // Phase C: build a fresh MapInstance per matching DTO via the canonical
            // ctor instead of MemberwiseClone (which previously reference-shared
            // Mobs / SummonMobs / KillSpawns lists across dungeon instances —
            // every party of the same dungeon would see the same mob HP state).
            long dungeonKey = IsParty
                ? (_partyManager.FindParty(client.TamerId)?.Id ?? 0L)
                : client.TamerId;

            if (IsParty && dungeonKey == 0L)
                return;

            if (_registry.FindByDungeonId(dungeonKey) != null)
                return;

            foreach (var dto in dtos)
            {
                if (dto.MapId != client.Tamer.Location.MapId)
                    continue;

                var cfg = _mapper.Map<MapConfigModel>(dto);
                var def = new MapDefinition(cfg);
                var newDungeon = new MapInstance(def, channelIdx: 0, cfg.Mobs, cfg.SummonMobs, cfg.KillSpawns);

                FilterDungeonMobs(newDungeon, dto.MapId);

                newDungeon.SetId((int)dungeonKey);

                _logger.Debug(IsParty
                    ? $"Initializing new instance for {def.Type} party {dungeonKey} - {def.Name}..."
                    : $"Initializing new instance for {def.Type} tamer {client.TamerId} - {def.Name}...");

                _registry.Register(MapTypeEnum.Dungeon, newDungeon);
                return; // one match per call
            }
        }

        private static void FilterDungeonMobs(MapInstance dungeon, int mapId)
        {
            var coliseumMobs = dungeon.Mobs.Where(x => x.Coliseum && x.Round > 0).ToList();
            foreach (var mob in coliseumMobs)
                dungeon.Mobs.Remove(mob);

            if (mapId == 2001 || mapId == 2002)
            {
                var wrongDay = dungeon.Mobs.Where(x => x.WeekDay != (DungeonDayOfWeekEnum)DateTime.Now.DayOfWeek).ToList();
                foreach (var mob in wrongDay)
                    dungeon.Mobs.Remove(mob);
            }
        }
        /// <summary>
        /// Gets the maps objects.
        /// </summary>
        public async Task GetMapObjects(CancellationToken cancellationToken)
        {

            await GetMapMobs(cancellationToken);
        }
        public async Task GetMapObjects()
        {

            await GetMapMobs();
        }

        /// <summary>
        /// Gets the map latest mobs.
        /// </summary>
        /// <returns>The mobs collection</returns>
        private async Task GetMapMobs(CancellationToken cancellationToken)
        {

            foreach (var map in Maps.Where(x => x.Initialized).ToList())
            {
                var mapMobs = _mapper.Map<IList<MobConfigModel>>(await _sender.Send(new MapMobConfigsQuery(map.MapId), cancellationToken));

                if(mapMobs != null)
                {
                    var mobsToRemove = mapMobs.Where(x => x.Coliseum && x.Round > 0).ToList();

                    if(mobsToRemove.Any())
                    {
                        foreach (var mob in mobsToRemove)
                        {
                            mapMobs.Remove(mob);
                        }
                    }
                }


                if (map.RequestMobsUpdate(mapMobs))
                    map.UpdateMobsList();
            }


        }
        private async Task GetMapMobs()
        {

            foreach (var map in Maps.Where(x => x.Initialized))
            {
                var mapMobs = _mapper.Map<IList<MobConfigModel>>(await _sender.Send(new MapMobConfigsQuery(map.MapId)));

                if (mapMobs != null)
                {
                    var mobsToRemove = mapMobs.Where(x => x.Coliseum && x.Round > 0).ToList();

                    if (mobsToRemove.Any())
                    {
                        foreach (var mob in mobsToRemove)
                        {
                            mapMobs.Remove(mob);
                        }
                    }
                }

                if (map.RequestMobsUpdate(mapMobs))
                    map.UpdateMobsList();
            }

        }

        /// <summary>
        /// Gets the consigned shops latest list.
        /// </summary>
        /// <returns>The consigned shops collection</returns>
        private async Task GetMapConsignedShops(CancellationToken cancellationToken)
        {
            //TODO: verificar necessidade de fazer o mesmo que nos mobs
            if (DateTime.Now > _lastConsignedShopsSearch)
            {
                foreach (var map in Maps.Where(x => x.Initialized))
                {
                    if (map.Operating)
                        continue;

                    var consignedShops = _mapper.Map<List<ConsignedShop>>(await _sender.Send(new ConsignedShopsQuery((int)map.Id), cancellationToken));

                    map.UpdateConsignedShops(consignedShops);
                }

                _lastConsignedShopsSearch = DateTime.Now.AddSeconds(15);
            }
        }

        /// <summary>
        /// The default hosted service "starting" method.
        /// </summary>
        /// <param name="cancellationToken">Control token for the operation</param>
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await CleanMaps();
                    await GetMapObjects(cancellationToken);

                    var tasks = new List<Task>();

                    Maps.ForEach(map => { tasks.Add(RunMap(map)); });

                    await Task.WhenAll(tasks);

                    await Task.Delay(500, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.Error($"Unexpected map exception: {ex.Message} {ex.StackTrace}");
                    await Task.Delay(3000, cancellationToken);
                }
            }
        }

        /// <summary>
        /// Runs the target map operations.
        /// </summary>
        /// <param name="map">the target map</param>
        private Task RunMap(MapInstance map)
            => _driver.RunMap(
                map,
                m => { TamerOperation(m); return Task.CompletedTask; },
                m => { MonsterOperation(m); return Task.CompletedTask; },
                m => { DropsOperation(m); return Task.CompletedTask; },
                _logger,
                CancellationToken.None);

        /// <summary>
        /// Adds a new gameclient to the target map.
        /// </summary>
        /// <param name="client">The game client to be added.</param>
        public async Task AddClient(GameClient client)
        {
            var party = _partyManager.FindParty(client.TamerId);

            if (party != null)
            {
                var partyMap = FindPartyDungeon(party, client.Tamer.Location.MapId);

                if (partyMap != null)
                {
                    client.SetLoading();
                    client.Tamer.MobsInView.Clear();
                    partyMap.AddClient(client);
                    _registry.OnTamerEnter(partyMap, client.TamerId);
                    client.Tamer.Revive();
                }
                else
                {
                    await SearchNewMaps(true, client);

                    await Task.Run(() =>
                    {
                        while (partyMap == null)
                        {
                            Thread.Sleep(2000);
                            partyMap = FindPartyDungeon(party, client.Tamer.Location.MapId);
                            _logger.Warning($"Waiting map {client.Tamer.Location.MapId} initialization.");
                        }

                        client.Tamer.MobsInView.Clear();
                        partyMap.AddClient(client);
                        _registry.OnTamerEnter(partyMap, client.TamerId);
                        client.Tamer.Revive();
                    });
                }
            }
            else
            {
                await SearchNewMaps(false, client);

                var map = FindSoloDungeon(client.TamerId);
                if (map != null)
                {
                    client.Tamer.MobsInView.Clear();
                    map.AddClient(client);
                    _registry.OnTamerEnter(map, client.TamerId);
                    client.Tamer.Revive();
                }
                else
                {
                    await Task.Run(() =>
                    {
                        while (map == null)
                        {
                            Thread.Sleep(2000);
                            map = FindSoloDungeon(client.TamerId);
                            _logger.Warning($"Waiting map {client.Tamer.Location.MapId} initialization.");
                        }

                        client.Tamer.MobsInView.Clear();
                        map.AddClient(client);
                        _registry.OnTamerEnter(map, client.TamerId);
                        client.Tamer.Revive();
                    });
                }
            }
        }

        /// <summary>
        /// Locate the initialized dungeon instance owned by <paramref name="party"/>
        /// whose MapId matches <paramref name="mapId"/>.  Tries both the leader-id
        /// (older scheme) and party-id (current) scope keys.
        /// </summary>
        private MapInstance? FindPartyDungeon(GameParty party, int mapId)
        {
            var leaderMap = _registry.FindByDungeonId(party.LeaderId);
            if (leaderMap?.Initialized == true && leaderMap.MapId == mapId)
                return leaderMap;

            var byPartyMap = _registry.FindByDungeonId(party.Id);
            if (byPartyMap?.Initialized == true && byPartyMap.MapId == mapId)
                return byPartyMap;

            return null;
        }

        private MapInstance? FindSoloDungeon(long tamerId)
        {
            var map = _registry.FindByDungeonId(tamerId);
            return map?.Initialized == true ? map : null;
        }

        /// <summary>
        /// Removes the gameclient from the target map.
        /// </summary>
        /// <param name="client">The gameclient to be removed.</param>
        public void RemoveClient(GameClient client)
        {
            var map = _registry.FindByTamer(client.TamerId);

            map?.RemoveClient(client);
            _registry.OnTamerLeave(client.TamerId);

            var party = _partyManager.FindParty(client.TamerId);
            if (party != null)
            {
                if (map?.Clients.Count == 0)
                    CleanMap(party.Id);
            }
            else
            {
                if (map?.Clients.Count == 0)
                    CleanMap((int)client.TamerId);
            }
        }

        // Phase D: shared broadcast/lookup helpers delegate to MapDriver.
        public void BroadcastForChannel(byte channel, byte[] packet)
            => _driver.BroadcastForChannel(_registry, channel, packet);

        public void BroadcastGlobal(byte[] packet)
            => _driver.BroadcastGlobal(_registry, packet);

        // Dungeons-only: broadcast scoped to the *tamer's* current dungeon
        // instance (since dungeons are per-party, multiple instances of the
        // same MapId can coexist).  Kept here — not a driver candidate.
        public void BroadcastForMap(short mapId, byte[] packet, long tamerId)
        {
            var map = _registry.FindByTamer(tamerId);
            map?.BroadcastForMap(packet);
        }

        public void BroadcastForUniqueTamer(long tamerId, byte[] packet)
            => _driver.BroadcastForUniqueTamer(_registry, tamerId, packet);

        public GameClient? FindClientByTamerId(long tamerId)
        {
            return Maps.SelectMany(map => map.Clients).FirstOrDefault(client => client.TamerId == tamerId);
        }

        public GameClient? FindClientByTamerName(string tamerName)
        {
            return Maps.SelectMany(map => map.Clients).FirstOrDefault(client => client.Tamer.Name == tamerName);
        }

        public GameClient? FindClientByTamerHandle(int handle)
        {
            return Maps.SelectMany(map => map.Clients).FirstOrDefault(client => client.Tamer?.GeneralHandler == handle);
        }

        public void BroadcastForTargetTamers(List<long> targetTamers, byte[] packet)
            => _driver.BroadcastForTargetTamers(_registry, targetTamers, packet);

        public void BroadcastForTargetTamers(long sourceId, byte[] packet)
            => _driver.BroadcastForTargetTamers(_registry, sourceId, packet);

        public void BroadcastForTamerViewsAndSelf(long sourceId, byte[] packet)
            => _driver.BroadcastForTamerViewsAndSelf(_registry, sourceId, packet);

        public void AddMapDrop(Drop drop,long tamerId)
        {
            var map = _registry.FindByTamer(tamerId);

            map?.DropsToAdd.Add(drop);
        }

        public void RemoveDrop(Drop drop,long tamerId)
        {
            var map = _registry.FindByTamer(tamerId);

            map?.RemoveMapDrop(drop);
        }

        public Drop? GetDrop(short mapId, int dropHandler, long tamerId)
        {
            var map = _registry.FindByTamer(tamerId);

            return map?.GetDrop(dropHandler);
        }

        //Mobs
        public bool MobsAttacking(short mapId, long tamerId)
        {
            var map = _registry.FindByTamer(tamerId);

            return map?.MobsAttacking(tamerId) ?? false;
        }
        public bool MobsAttacking(short mapId, long tamerId, bool Summon)
        {
            var map = _registry.FindByTamer(tamerId);

            return map?.MobsAttacking(tamerId) ?? false;
        }
        public List<CharacterModel> GetNearbyTamers(short mapId, long tamerId)
        {
            var map = _registry.FindByTamer(tamerId);

            return map?.NearbyTamers(tamerId);
        }
        public void AddSummonMobs(short mapId, SummonMobModel summon, long tamerId)
        {
            var map = _registry.FindByTamer(tamerId);

            map?.AddMob(summon);
        }
        public void AddMobs(short mapId, MobConfigModel mob,long tamerId)
        {
            var map = _registry.FindByTamer(tamerId);

            map?.AddMob(mob);
        }
        public MobConfigModel? GetMobByHandler(short mapId, int handler,long tamerId)
        {
            var map = Maps.
                  FirstOrDefault(x => x.Clients.Exists(x => x.TamerId == tamerId));

            if (map == null)
                return null;

            return map.Mobs
            .FirstOrDefault(x => x.GeneralHandler == handler);
        }
        public SummonMobModel? GetMobByHandler(short mapId, int handler, bool summon,long tamerId)
        {
           var map = Maps.
                FirstOrDefault(x => x.Clients.Exists(x => x.TamerId == tamerId));

            if (map == null)
                return null;

                return map.SummonMobs
                .FirstOrDefault(x => x.GeneralHandler == handler);
        }
        public List<MobConfigModel> GetMobsNearbyPartner(Location location, int range,long tamerId)
        {
            var targetMap = _registry.FindByTamer(tamerId);

            if (targetMap == null)
                return default;

            var originX = location.X;
            var originY = location.Y;

            return GetTargetMobs(targetMap.Mobs.Where(x => x.Alive).ToList(), originX, originY, range).DistinctBy(x => x.Id).ToList();
        }

        public List<MobConfigModel> GetMobsNearbyTargetMob(short mapId, int handler, int range,long tamerId)
        {
            var targetMap = _registry.FindByTamer(tamerId);
            if (targetMap == null)
                return default;

            var originMob = targetMap.Mobs.FirstOrDefault(x => x.GeneralHandler == handler);

            if (originMob == null)
                return default;

            var originX = originMob.CurrentLocation.X;
            var originY = originMob.CurrentLocation.Y;

            var targetMobs = new List<MobConfigModel>();
            targetMobs.Add(originMob);

            targetMobs.AddRange(GetTargetMobs(targetMap.Mobs.Where(x => x.Alive).ToList(), originX, originY, range));

            return targetMobs.DistinctBy(x => x.Id).ToList();
        }

        public static List<MobConfigModel> GetTargetMobs(List<MobConfigModel> mobs, int originX, int originY, int range)
        {
            var targetMobs = new List<MobConfigModel>();

            foreach (var mob in mobs)
            {
                var mobX = mob.CurrentLocation.X;
                var mobY = mob.CurrentLocation.Y;

                var distance = CalculateDistance(originX, originY, mobX, mobY);

                if (distance <= range)
                {
                    targetMobs.Add(mob);
                }
            }

            return targetMobs;
        }


        public List<SummonMobModel> GetMobsNearbyPartner(Location location, int range, bool Summon,long tamerId)
        {
            var targetMap = _registry.FindByTamer(tamerId);

            if (targetMap == null)
                return default;

            var originX = location.X;
            var originY = location.Y;

            return GetTargetMobs(targetMap.SummonMobs.Where(x => x.Alive).ToList(), originX, originY, range).DistinctBy(x => x.Id).ToList();
        }

        public List<SummonMobModel> GetMobsNearbyTargetMob(short mapId, int handler, int range, bool Summon,long tamerId)
        {
            var targetMap = _registry.FindByTamer(tamerId);

            if (targetMap == null)
                return default;

            var originMob = targetMap.SummonMobs.FirstOrDefault(x => x.GeneralHandler == handler);

            if (originMob == null)
                return default;

            var originX = originMob.CurrentLocation.X;
            var originY = originMob.CurrentLocation.Y;

            var targetMobs = new List<SummonMobModel>();
            targetMobs.Add(originMob);

            targetMobs.AddRange(GetTargetMobs(targetMap.SummonMobs.Where(x => x.Alive).ToList(), originX, originY, range));

            return targetMobs.DistinctBy(x => x.Id).ToList();
        }

        public static List<SummonMobModel> GetTargetMobs(List<SummonMobModel> mobs, int originX, int originY, int range)
        {
            var targetMobs = new List<SummonMobModel>();

            foreach (var mob in mobs)
            {
                var mobX = mob.CurrentLocation.X;
                var mobY = mob.CurrentLocation.Y;

                var distance = CalculateDistance(originX, originY, mobX, mobY);

                if (distance <= range)
                {
                    targetMobs.Add(mob);
                }
            }

            return targetMobs;
        }


        private static double CalculateDistance(int x1, int y1, int x2, int y2)
        {
            var deltaX = x2 - x1;
            var deltaY = y2 - y1;
            return Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
        }
    }
}
