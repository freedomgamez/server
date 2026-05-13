using DigitalWorldOnline.Application.Separar.Queries;
using DigitalWorldOnline.Application.GameAssets.Queries;
using DigitalWorldOnline.Commons.Entities;
using DigitalWorldOnline.Commons.Enums;
using DigitalWorldOnline.Commons.Models;
using DigitalWorldOnline.Commons.Models.Character;
using DigitalWorldOnline.Commons.Models.Config;
using DigitalWorldOnline.Commons.Models.Map;
using DigitalWorldOnline.Commons.Models.Summon;
using DigitalWorldOnline.Commons.Models.TamerShop;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using System.Linq;

namespace DigitalWorldOnline.GameHost
{
    public sealed partial class MapServer
    {
        private DateTime _lastMapsSearch = DateTime.Now;
        private DateTime _lastMobsSearch = DateTime.Now;
        private DateTime _lastConsignedShopsSearch = DateTime.Now;

        //TODO: externalizar
        private readonly int _startToSee = 6000;
        private readonly int _stopSeeing = 6001;
      
        /// <summary>
        /// Cleans unused running maps.  Lifecycle hoisted into <see cref="DefaultMapDriver"/>.
        /// </summary>
        public Task CleanMaps()
        {
            _driver.CleanIdle(_registry, _logger);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Periodic poll — every 10 s the driver loads any new map definitions
        /// from DB (Phase F: bin) and spawns 3 channel instances per map.
        /// </summary>
        public async Task SearchNewMaps(CancellationToken cancellationToken)
        {
            if (DateTime.Now > _lastMapsSearch)
            {
                await _driver.RefreshInstances(_sender, _mapper, _registry, _logger, cancellationToken);
                _lastMapsSearch = DateTime.Now.AddSeconds(10);
            }
        }

        /// <summary>
        /// Gets the maps objects.
        /// </summary>
        public async Task GetMapObjects(CancellationToken cancellationToken)
        {
            await GetMapConsignedShops(cancellationToken);
            await GetMapMobs(cancellationToken);
        }

        /// <summary>
        /// Gets the map latest mobs.
        /// </summary>
        /// <returns>The mobs collection</returns>
        private async Task GetMapMobs(CancellationToken cancellationToken)
        {
            if (DateTime.Now > _lastMobsSearch)
            {
                foreach (var map in Maps.Where(x => x.Initialized))
                {
                    var mapMobs = _mapper.Map<IList<MobConfigModel>>(await _sender.Send(new MapMobConfigsQuery(map.MapId), cancellationToken));

                    if (map.RequestMobsUpdate(mapMobs))
                        map.UpdateMobsList();
                }

                _lastMobsSearch = DateTime.Now.AddSeconds(30);
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

                    using var scope = _scopeFactory.CreateScope();
                    var serverQueriesRepository = scope.ServiceProvider.GetRequiredService<Commons.Interfaces.IServerQueriesRepository>();
                    var consignedShopDtos = await serverQueriesRepository.GetConsignedShopsAsync((int)map.Id);
                    var consignedShops = _mapper.Map<List<ConsignedShop>>(consignedShopDtos);

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
                    await SearchNewMaps(cancellationToken);
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
        /// Per-tick body — delegates to <see cref="MapDriver.RunMap"/>.  The
        /// driver decides parallelism + tail-delay shape; the operation methods
        /// (<see cref="TamerOperation"/> / <see cref="MonsterOperation"/> /
        /// <see cref="DropsOperation"/>) are passed in as callbacks because
        /// their bodies are MapServer-specific.
        /// </summary>
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
        public Task AddClient(GameClient client)
        {
            var map = PickChannelFor(client);

            client.SetLoading();

            if (map != null)
            {
                client.Tamer.MobsInView.Clear();
                map.AddClient(client);
                _registry.OnTamerEnter(map, client.TamerId);
                client.Tamer.Revive();
            }
            else
            {
                Task.Run(() =>
                {
                    var stopWatch = Stopwatch.StartNew();
                    var timeLimit = 15000;
                    while (map == null)
                    {
                        Thread.Sleep(2000);
                        map = PickChannelFor(client);

                        _logger.Warning($"Waiting map {client.Tamer.Location.MapId} initialization.");

                        if (stopWatch.ElapsedMilliseconds >= timeLimit)
                        {
                            _logger.Warning($"A instancia do mapa {client.Tamer.Location.MapId} não foi iniciada, abortando processo...");
                            stopWatch.Stop();
                            break;
                        }
                    }

                    if (map == null)
                    {
                        client.Disconnect();
                    }
                    else
                    {
                        client.Tamer.MobsInView.Clear();
                        map.AddClient(client);
                        _registry.OnTamerEnter(map, client.TamerId);
                        client.Tamer.Revive();
                    }
                });
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Removes the gameclient from the target map.
        /// </summary>
        /// <param name="client">The gameclient to be removed.</param>
        public void RemoveClient(GameClient client)
        {
            // Tamer-bound — registry's tamer-id cache is O(1) and channel-correct.
            // (Old flat scan on MapId picked an arbitrary channel of that map.)
            var map = _registry.FindByTamer(client.TamerId);
            map?.RemoveClient(client);
            _registry.OnTamerLeave(client.TamerId);
        }

        /// <summary>
        /// Public O(1) tamer→map lookup for callers outside this class (packet
        /// processors etc.).  Wraps <see cref="MapRegistry.FindByTamer"/> so
        /// callers don't need to inject the registry directly.
        /// </summary>
        public MapInstance? FindMapByTamer(long tamerId) => _registry.FindByTamer(tamerId);

        /// <summary>
        /// Phase E Step 6 — pick which channel a freshly-entering client should
        /// land on.  Priority:
        /// <list type="number">
        ///   <item>If <c>client.Tamer.Channel</c> points at a live, initialized
        ///   channel of this map (e.g. just came back from a channel switch or
        ///   relog), honour it.</item>
        ///   <item>Otherwise pick the lowest-populated channel via the
        ///   registry.</item>
        ///   <item>Fallback: any initialized channel of the map (matches old
        ///   behaviour when the registry hasn't seen the map yet).</item>
        /// </list>
        /// Returns <c>null</c> if no channel for the map exists yet — the
        /// caller's spin loop will wait for SearchNewMaps to register one.
        /// </summary>
        private MapInstance? PickChannelFor(GameClient client)
        {
            var mapId = client.Tamer.Location.MapId;
            var preferred = _registry.GetByMapAndChannel(MapTypeEnum.Default, mapId, client.Tamer.Channel);
            if (preferred != null && preferred.Initialized)
                return preferred;

            var balanced = _registry.PickLowestPopulated(MapTypeEnum.Default, mapId);
            if (balanced != null)
            {
                // Persist the chosen channel so later lookups (and a relog)
                // hit the same one.  Fire-and-forget — no need to await.
                client.Tamer.SetCurrentChannel(balanced.Channel);
                return balanced;
            }

            return Maps.FirstOrDefault(x => x.Initialized && x.MapId == mapId);
        }

        // Phase D: broadcast / lookup helpers all delegate to the driver, which
        // does the same registry-keyed work for every server type.
        public void BroadcastForChannel(byte channel, byte[] packet)
            => _driver.BroadcastForChannel(_registry, channel, packet);

        public void BroadcastGlobal(byte[] packet)
            => _driver.BroadcastGlobal(_registry, packet);

        public void BroadcastForSelectedMaps(byte[] packet, List<int> mapIds)
        {
            // MapServer-only — the other servers don't have multi-map selective
            // broadcast.  Iterate the default-type channels and filter to the
            // requested MapId set.
            foreach (var map in _registry.GetByType(MapTypeEnum.Default))
                if (map.Clients.Any() && mapIds.Contains(map.MapId))
                    map.BroadcastForMap(packet);
        }

        public void BroadcastForMap(short mapId, byte[] packet)
            => _driver.BroadcastForMap(_registry, mapId, packet);

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

        public void AddMapDrop(Drop drop) => _driver.AddMapDrop(_registry, drop);
        public void RemoveDrop(Drop drop) => _driver.RemoveDrop(_registry, drop);
        public Drop? GetDrop(short mapId, int dropHandler) => _driver.GetDrop(_registry, mapId, dropHandler);

        //Mobs
        public bool MobsAttacking(short mapId, long tamerId)
            => _driver.MobsAttacking(_registry, mapId, tamerId);

        public bool MobsAttacking(short mapId, long tamerId, bool Summon)
        {
            // MapServer-only summon overload — distinct semantic (summon mobs not
            // catalog mobs); kept here.
            var map = _registry.FindByTamer(tamerId);
            return map?.MobsAttacking(tamerId, true) ?? false;
        }

        public List<CharacterModel> GetNearbyTamers(short mapId, long tamerId)
            => _driver.GetNearbyTamers(_registry, mapId, tamerId);

        public void AddSummonMobs(short mapId, SummonMobModel summon)
            => _driver.AddSummonMobs(_registry, mapId, summon);

        public MobConfigModel? GetMobByHandler(short mapId, int handler)
            => _driver.GetMobByHandler(_registry, mapId, handler);

        public SummonMobModel? GetMobByHandler(short mapId, int handler, bool summon)
        {
            // MapServer-only summon overload — keep here.
            var map = _registry.GetChannelsOf(MapTypeEnum.Default, mapId).FirstOrDefault();
            return map?.SummonMobs.FirstOrDefault(x => x.GeneralHandler == handler);
        }

        public List<MobConfigModel> GetMobsNearbyPartner(Location location, int range)
            => _driver.GetMobsNearbyPartner(_registry, location, range);

        public List<MobConfigModel> GetMobsNearbyTargetMob(short mapId, int handler, int range)
            => _driver.GetMobsNearbyTargetMob(_registry, mapId, handler, range);

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


        public List<SummonMobModel> GetMobsNearbyPartner(Location location, int range, bool Summon)
        {
            // TODO Phase E: needs requesting partner's channel.
            var targetMap = _registry.GetChannelsOf(MapTypeEnum.Default, location.MapId).FirstOrDefault();
            if (targetMap == null)
                return new List<SummonMobModel>();

            var originX = location.X;
            var originY = location.Y;

            return GetTargetMobs(targetMap.SummonMobs.Where(x => x.Alive).ToList(), originX, originY, range).DistinctBy(x => x.Id).ToList();
        }

        public List<SummonMobModel> GetMobsNearbyTargetMob(short mapId, int handler, int range, bool Summon)
        {
            // TODO Phase E: channel ambiguity.
            var targetMap = _registry.GetChannelsOf(MapTypeEnum.Default, mapId).FirstOrDefault();
            if (targetMap == null)
                return new List<SummonMobModel>();

            var originMob = targetMap.SummonMobs.FirstOrDefault(x => x.GeneralHandler == handler);

            if (originMob == null)
                return new List<SummonMobModel>();

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
