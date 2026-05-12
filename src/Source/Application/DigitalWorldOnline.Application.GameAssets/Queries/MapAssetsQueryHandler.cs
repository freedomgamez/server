using DigitalWorldOnline.Application.GameAssets.Bins;
using DigitalWorldOnline.Commons.DTOs.Assets;
using MediatR;

namespace DigitalWorldOnline.Application.GameAssets.Queries
{
    public class MapAssetsQueryHandler : IRequestHandler<MapAssetsQuery, List<MapAssetDTO>>
    {
        private readonly MapBinLoader _mapBin;

        public MapAssetsQueryHandler(MapBinLoader mapBin)
        {
            _mapBin = mapBin;
        }

        public Task<List<MapAssetDTO>> Handle(MapAssetsQuery request, CancellationToken cancellationToken)
        {
            if (_mapBin.IsLoaded)
            {
                return Task.FromResult(_mapBin.Data.MapsById.Values
                    .Select(map => new MapAssetDTO
                    {
                        Id = map.MapId,
                        MapId = map.MapId,
                        Name = $"Map {map.MapId}",
                        RegionIndex = (byte)Math.Min(byte.MaxValue, map.MapRegionId)
                    })
                    .OrderBy(map => map.MapId)
                    .ToList());
            }

            throw new InvalidOperationException(
                "Map static assets must come from MapList.bin (MapAssetsQuery).");
        }
    }
}
