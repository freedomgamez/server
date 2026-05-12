using DigitalWorldOnline.Application.GameAssets.Bins;
using DigitalWorldOnline.Commons.DTOs.Config;
using DigitalWorldOnline.Commons.Enums;
using MediatR;

namespace DigitalWorldOnline.Application.Separar.Queries
{
    public class GameMapConfigByMapIdQueryHandler : IRequestHandler<GameMapConfigByMapIdQuery, MapConfigDTO?>
    {
        private readonly MapBinLoader _mapBin;

        public GameMapConfigByMapIdQueryHandler(MapBinLoader mapBin)
        {
            _mapBin = mapBin;
        }

        public Task<MapConfigDTO?> Handle(GameMapConfigByMapIdQuery request, CancellationToken cancellationToken)
        {
            if (_mapBin.IsLoaded && _mapBin.Data.MapsById.TryGetValue(request.MapId, out var map))
            {
                return Task.FromResult<MapConfigDTO?>(new MapConfigDTO
                {
                    Id = map.MapId,
                    MapId = map.MapId,
                    Name = $"Map {map.MapId}",
                    Type = MapTypeEnum.Default,
                    Mobs = new(),
                    KillSpawns = new()
                });
            }

            throw new InvalidOperationException(
                $"Map static data missing in bins for map {request.MapId} (GameMapConfigByMapIdQuery).");
        }
    }
}
