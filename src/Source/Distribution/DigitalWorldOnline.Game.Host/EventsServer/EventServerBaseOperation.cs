using DigitalWorldOnline.Application.Separar.Queries;
using DigitalWorldOnline.Application.GameAssets.Queries;
using DigitalWorldOnline.Commons.Entities;
using DigitalWorldOnline.Commons.Enums;
using DigitalWorldOnline.Commons.Models;
using DigitalWorldOnline.Commons.Models.Character;
using DigitalWorldOnline.Commons.Models.Config;
using DigitalWorldOnline.Commons.Models.Map;


namespace DigitalWorldOnline.GameHost.EventsServer
{
    public sealed partial class EventServer
    {
        private readonly int _startToSee = 8000;
        private readonly int _stopSeeing = 8001;

        /// <summary>
        /// The default hosted service "starting" method.
        /// </summary>
        /// <param name="cancellationToken">Control token for the operation</param>
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            // Phase D: wire the driver's pre-init hook to our event-specific
            // mob seeding so the driver can run the standard tick body.
            _driver.OnFirstTick = async (map, token) =>
            {
                var mobs = await GetMapMobs(map, token);
                map.RequestMobsUpdate(mobs);
                map.UpdateMobsList();
            };

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var tasks = new List<Task>();

                    Maps.ForEach(map => { tasks.Add(RunMap(map, cancellationToken)); });

                    await Task.WhenAll(tasks);

                    await Task.Delay(300, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.Error($"Unexpected map exception: {ex.Message} {ex.StackTrace}");
                    await Task.Delay(3000, cancellationToken);
                }
            }
        }

        /// <summary>
        /// Gets the map latest mobs.
        /// </summary>
        /// <returns>The mobs collection</returns>
        private async Task<List<MobConfigModel>> GetMapMobs(MapInstance map, CancellationToken token)
        {
            var mobList = new List<MobConfigModel>();

            var mobs = _mapper.Map<List<MobConfigModel>>(await _sender.Send(new MapMobsByIdQuery(map.MapId), token));

            if (!mobs.Any())
                return mobList;

            var id = 1;
            while (mobList.Count < MobAmount)
            {
                mobs.ForEach(mob =>
                {
                    var newMob = (MobConfigModel)mob.Clone();

                    var location = _randomPoints.First(x => x.Free);
                    location.UsePoint();

                    newMob.SetLocation((short)map.MapId, location.X, location.Y);
                    newMob.SetId(id);
                    id++;

                    mobList.Add(newMob);
                });
            }

            return mobList;
        }

        /// <summary>
        /// Runs the target map operations.
        /// </summary>
        /// <param name="map">the target map</param>
        // Phase D: per-tick body delegates to EventMapDriver.RunMap.  The
        // driver's OnFirstTick hook (wired in StartAsync) runs the event-only
        // pre-init mob seeding before MapInstance.Initialize.
        private Task RunMap(MapInstance map, CancellationToken token)
            => _driver.RunMap(
                map,
                TamerOperation,
                MonsterOperation,
                DropsOperation,
                _logger,
                token);

        /// <summary>
        /// Adds a new gameclient to the target map.
        /// </summary>
        /// <param name="client">The game client to be added.</param>
        public Task AddClient(GameClient client)
        {
            var map = Maps
                    .FirstOrDefault(x => x.Initialized &&
                                         x.MapId == client.Tamer.Location.MapId);

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
                    while (map == null)
                    {
                        Thread.Sleep(2000);
                        map = Maps
                            .FirstOrDefault(x => x.Initialized &&
                                                 x.MapId == client.Tamer.Location.MapId);

                        _logger.Warning($"Waiting map {client.Tamer.Location.MapId} initialization.");
                    }

                    client.Tamer.MobsInView.Clear();
                    map.AddClient(client);
                    _registry.OnTamerEnter(map, client.TamerId);
                    client.Tamer.Revive();
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
            var map = _registry.FindByTamer(client.TamerId)
                ?? _registry.GetChannelsOf(MapTypeEnum.Event, client.Tamer.Location.MapId).FirstOrDefault();

            map?.RemoveClient(client);
            _registry.OnTamerLeave(client.TamerId);
        }

        // Phase D: broadcast / lookup helpers delegate to MapDriver.
        public void BroadcastForChannel(byte channel, byte[] packet)
            => _driver.BroadcastForChannel(_registry, channel, packet);

        public void BroadcastGlobal(byte[] packet)
            => _driver.BroadcastGlobal(_registry, packet);

        public void BroadcastForMap(short mapId, byte[] packet)
            => _driver.BroadcastForMap(_registry, mapId, packet);

        public void BroadcastForUniqueTamer(long tamerId, byte[] packet)
            => _driver.BroadcastForUniqueTamer(_registry, tamerId, packet);

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

        public List<CharacterModel> GetNearbyTamers(short mapId, long tamerId)
            => _driver.GetNearbyTamers(_registry, mapId, tamerId);

        public MobConfigModel? GetMobByHandler(short mapId, int handler)
            => _driver.GetMobByHandler(_registry, mapId, handler);

        public List<MobConfigModel> GetMobsNearbyPartner(Location location, int range)
            => _driver.GetMobsNearbyPartner(_registry, location, range);

        public List<MobConfigModel> GetMobsNearbyTargetMob(short mapId, int handler, int range)
            => _driver.GetMobsNearbyTargetMob(_registry, mapId, handler, range);

        // GetTargetMobs / CalculateDistance helpers retired — MapDriver computes
        // Euclidean distance inline.  See MapDriver.EuclideanWithin.
    }
}
