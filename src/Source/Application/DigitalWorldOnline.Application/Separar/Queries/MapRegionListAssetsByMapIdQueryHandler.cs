using DigitalWorldOnline.Application.GameAssets.Bins;
using DigitalWorldOnline.Commons.DTOs.Assets;
using MediatR;

namespace DigitalWorldOnline.Application.Separar.Queries
{
    public class MapRegionListAssetsByMapIdQueryHandler : IRequestHandler<MapRegionListAssetsByMapIdQuery, MapRegionListAssetDTO?>
    {
        private readonly MapBinLoader _mapBin;

        public MapRegionListAssetsByMapIdQueryHandler(MapBinLoader mapBin)
        {
            _mapBin = mapBin;
        }

        public Task<MapRegionListAssetDTO?> Handle(MapRegionListAssetsByMapIdQuery request, CancellationToken cancellationToken)
        {
            if (_mapBin.IsLoaded && _mapBin.Data.RegionsByMapId.TryGetValue(request.MapId, out var regions) && regions.Any())
            {
                var mappedRegions = new List<MapRegionAssetDTO>(regions.Count);
                for (int index = 0; index < regions.Count; index++)
                {
                    var region = regions[index];
                    mappedRegions.Add(new MapRegionAssetDTO
                    {
                        Id = index + 1,
                        Index = (byte)index,
                        X = region.CenterX,
                        Y = region.CenterY,
                        Name = $"Map {request.MapId} Region {index + 1}",
                        MapRegionListId = request.MapId
                    });
                }

                return Task.FromResult<MapRegionListAssetDTO?>(new MapRegionListAssetDTO
                {
                    Id = request.MapId,
                    MapId = request.MapId,
                    Name = $"Map {request.MapId}",
                    Regions = mappedRegions
                });
            }

            throw new InvalidOperationException(
                $"Map static data missing in bins for map {request.MapId} (MapRegionListAssetsByMapIdQuery).");
        }
    }
}
