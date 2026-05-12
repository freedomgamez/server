using DigitalWorldOnline.Application.GameAssets.Bins;
using DigitalWorldOnline.Commons.DTOs.Config;
using DigitalWorldOnline.Commons.Enums;
using MediatR;

namespace DigitalWorldOnline.Application.Separar.Queries
{
    public class GameMapConfigByIdQueryHandler : IRequestHandler<GameMapConfigByIdQuery, MapConfigDTO?>
    {
        private readonly MapBinLoader _mapBin;

        public GameMapConfigByIdQueryHandler(MapBinLoader mapBin)
        {
            _mapBin = mapBin;
        }

        public Task<MapConfigDTO?> Handle(GameMapConfigByIdQuery request, CancellationToken cancellationToken)
        {
            int mapId = (int)request.Id;
            if (_mapBin.IsLoaded && _mapBin.Data.MapsById.ContainsKey(mapId))
            {
                return Task.FromResult<MapConfigDTO?>(new MapConfigDTO
                {
                    Id = mapId,
                    MapId = mapId,
                    Name = $"Map {mapId}",
                    Type = MapTypeEnum.Default,
                    Mobs = new(),
                    KillSpawns = new()
                });
            }

            throw new InvalidOperationException(
                $"Map static config missing in bins for map {request.Id} (GameMapConfigByIdQuery).");
        }
    }
}
