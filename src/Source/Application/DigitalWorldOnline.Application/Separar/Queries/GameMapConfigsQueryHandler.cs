using DigitalWorldOnline.Application.GameAssets.Bins;
using DigitalWorldOnline.Commons.DTOs.Config;
using DigitalWorldOnline.Commons.Enums;
using MediatR;

namespace DigitalWorldOnline.Application.Separar.Queries
{
    public class GameMapConfigsQueryHandler : IRequestHandler<GameMapConfigsQuery, IList<MapConfigDTO>>
    {
        private readonly MapBinLoader _mapBin;

        public GameMapConfigsQueryHandler(MapBinLoader mapBin)
        {
            _mapBin = mapBin;
        }

        public Task<IList<MapConfigDTO>> Handle(GameMapConfigsQuery request, CancellationToken cancellationToken)
        {
            if (_mapBin.IsLoaded)
            {
                IList<MapConfigDTO> maps = _mapBin.Data.MapsById.Values
                    .OrderBy(x => x.MapId)
                    .Select(map => new MapConfigDTO
                    {
                        Id = map.MapId,
                        MapId = map.MapId,
                        Name = $"Map {map.MapId}",
                        Type = MapTypeEnum.Default,
                        Mobs = new(),
                        KillSpawns = new()
                    })
                    .ToList();

                return Task.FromResult(maps);
            }

            throw new InvalidOperationException("Map static configs must come from MapList.bin (GameMapConfigsQuery).");
        }
    }
}
