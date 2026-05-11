using DigitalWorldOnline.Application.Separar.Queries;
using DigitalWorldOnline.Application.GameAssets.Queries;
using DigitalWorldOnline.Commons.Entities;
using DigitalWorldOnline.Commons.Enums;
using DigitalWorldOnline.Commons.Models;
using DigitalWorldOnline.Commons.Models.Character;
using DigitalWorldOnline.Commons.Models.Config;
using DigitalWorldOnline.Commons.Models.Digimon;
using DigitalWorldOnline.Commons.Models.Map;
using System.Diagnostics;

namespace DigitalWorldOnline.GameHost
{
    public sealed partial class PvpServer
    {
        private DateTime _lastMapsSearch = DateTime.Now;

        /// <summary>
        /// Cleans unused running maps.
        /// </summary>
        public Task CleanMaps()
        {
            _driver.CleanIdle(_registry, _logger);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Periodic PvP map lifecycle — delegated to <see cref="PvpMapDriver"/>.
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

                    var tasks = new List<Task>();

                    Maps.ForEach(map => { tasks.Add(RunMap(map)); });

                    await Task.WhenAll(tasks);

                    await Task.Delay(500, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.Error($"Unexpected PvP map exception: {ex.Message} {ex.StackTrace}");
                    await Task.Delay(3000, cancellationToken);
                }
            }
        }

        /// <summary>
        /// Runs the target map operations.
        /// </summary>
        /// <param name="map">the target map</param>
        // Pvp tick: PvpMapDriver overrides RunMap to ignore the monster/drops
        // callbacks (Pvp arenas don't have NPC mobs or world drops).  Pass
        // no-ops there.
        private Task RunMap(MapInstance map)
            => _driver.RunMap(
                map,
                m => { TamerOperation(m); return Task.CompletedTask; },
                _ => Task.CompletedTask,
                _ => Task.CompletedTask,
                _logger,
                CancellationToken.None);

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
                        _logger.Warning($"Waiting map {client.Tamer.Location.MapId} initialization.");

                        Thread.Sleep(2000);
                        map = Maps
                            .FirstOrDefault(x => x.Initialized &&
                                                 x.MapId == client.Tamer.Location.MapId);
                    }

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
                ?? _registry.GetChannelsOf(MapTypeEnum.Pvp, client.Tamer.Location.MapId).FirstOrDefault();

            map?.RemoveClient(client);
            _registry.OnTamerLeave(client.TamerId);
        }

        // Phase D: broadcast helpers delegate to driver.
        public void BroadcastForChannel(byte channel, byte[] packet)
            => _driver.BroadcastForChannel(_registry, channel, packet);

        public void BroadcastGlobal(byte[] packet)
            => _driver.BroadcastGlobal(_registry, packet);

        public void BroadcastForMap(short mapId, byte[] packet)
            => _driver.BroadcastForMap(_registry, mapId, packet);

        public void BroadcastForUniqueTamer(long tamerId, byte[] packet)
            => _driver.BroadcastForUniqueTamer(_registry, tamerId, packet);

        public GameClient? FindClientByTamerId(long tamerId)
            => Maps.SelectMany(map => map.Clients).FirstOrDefault(client => client.TamerId == tamerId);

        public GameClient? FindClientByTamerName(string tamerName)
            => Maps.SelectMany(map => map.Clients).FirstOrDefault(client => client.Tamer.Name == tamerName);

        public void BroadcastForTargetTamers(List<long> targetTamers, byte[] packet)
            => _driver.BroadcastForTargetTamers(_registry, targetTamers, packet);

        public void BroadcastForTargetTamers(long sourceId, byte[] packet)
            => _driver.BroadcastForTargetTamers(_registry, sourceId, packet);

        public void BroadcastForTamerViewsAndSelf(long sourceId, byte[] packet)
            => _driver.BroadcastForTamerViewsAndSelf(_registry, sourceId, packet);

        public bool EnemiesAttacking(short mapId, long partnerId)
        {
            var map = _registry.FindByTamer(partnerId)
                ?? _registry.GetChannelsOf(MapTypeEnum.Pvp, mapId).FirstOrDefault();

            return map?.PlayersAttacking(partnerId) ?? false;
        }

        public DigimonModel? GetEnemyByHandler(short mapId, int handler)
        {
            return _registry.GetChannelsOf(MapTypeEnum.Pvp, mapId).FirstOrDefault()?
                .ConnectedTamers
                .Select(x => x.Partner)
                .FirstOrDefault(x => x.GeneralHandler == handler);
        }

        public List<MobConfigModel> GetMobsNearbyPartner(Location location, int range)
            => _driver.GetMobsNearbyPartner(_registry, location, range);

        public List<MobConfigModel> GetMobsNearbyTargetMob(short mapId, int handler, int range)
            => _driver.GetMobsNearbyTargetMob(_registry, mapId, handler, range);

        // GetTargetMobs / CalculateDistance retired — see MapDriver.EuclideanWithin.
    }
}
