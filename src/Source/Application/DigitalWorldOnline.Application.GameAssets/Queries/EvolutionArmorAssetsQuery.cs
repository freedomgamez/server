using MediatR;
using DigitalWorldOnline.Commons.DTOs.Assets;
using DigitalWorldOnline.Commons.DTOs.Config;

namespace DigitalWorldOnline.Application.GameAssets.Queries
{
    public class EvolutionArmorAssetsQuery : IRequest<List<EvolutionArmorAssetDTO>>
    {
    }
}

