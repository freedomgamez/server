using DigitalWorldOnline.Application.GameAssets.Bins;
using DigitalWorldOnline.Commons.DTOs.Assets;
using DigitalWorldOnline.Commons.Enums.ClientEnums;
using MediatR;

namespace DigitalWorldOnline.Application.GameAssets.Queries
{
    public class PortalAssetsQueryHandler : IRequestHandler<PortalAssetsQuery, List<PortalAssetDTO>>
    {
        private readonly MapBinLoader _mapBin;

        public PortalAssetsQueryHandler(MapBinLoader mapBin)
        {
            _mapBin = mapBin;
        }

        public Task<List<PortalAssetDTO>> Handle(PortalAssetsQuery request, CancellationToken cancellationToken)
        {
            if (_mapBin.IsLoaded)
            {
                return Task.FromResult(_mapBin.Data.PortalsBySourceMapId.Values
                    .SelectMany(portals => portals)
                    .Select(portal => new PortalAssetDTO
                    {
                        Id = portal.PortalId,
                        Type = portal.PortalType == 1 ? PortalTypeEnum.Dungeon : PortalTypeEnum.Normal,
                        NpcId = portal.UniqObjectId,
                        DestinationMapId = portal.DestMapId,
                        DestinationX = portal.DestX,
                        DestinationY = portal.DestY,
                        PortalIndex = portal.PortalKindIndex
                    })
                    .OrderBy(portal => portal.Id)
                    .ToList());
            }

            throw new InvalidOperationException(
                "Portal static assets must come from MapPortal.bin (PortalAssetsQuery).");
        }
    }
}
