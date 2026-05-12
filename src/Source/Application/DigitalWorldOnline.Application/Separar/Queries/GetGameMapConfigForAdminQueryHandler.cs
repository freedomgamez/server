using DigitalWorldOnline.Application.GameAssets.Bins;
using MediatR;

namespace DigitalWorldOnline.Application.Separar.Queries
{
    public class GetGameMapConfigForAdminQueryHandler : IRequestHandler<GetGameMapConfigForAdminQuery, List<GetGameMapConfigForAdminQueryDto>>
    {
        private readonly MapBinLoader _mapBin;

        public GetGameMapConfigForAdminQueryHandler(MapBinLoader mapBin)
        {
            _mapBin = mapBin;
        }

        public Task<List<GetGameMapConfigForAdminQueryDto>> Handle(GetGameMapConfigForAdminQuery request, CancellationToken cancellationToken)
        {
            if (_mapBin.IsLoaded)
            {
                var maps = _mapBin.Data.MapsById.Values
                    .OrderBy(x => x.MapId)
                    .Select(map => new GetGameMapConfigForAdminQueryDto
                    {
                        Id = map.MapId,
                        MapId = map.MapId,
                        Name = $"Map {map.MapId}",
                        Mobs = _mapBin.Data.MonstersByMapId.TryGetValue(map.MapId, out var mobs) ? mobs.Count : 0
                    })
                    .ToList();

                return Task.FromResult(maps);
            }

            throw new InvalidOperationException("Admin map static data must come from bins (GetGameMapConfigForAdminQuery).");
        }
    }
}
