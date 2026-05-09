using MediatR;
using DigitalWorldOnline.Commons.DTOs.Assets;

namespace DigitalWorldOnline.Application.GameAssets.Queries
{
    public class ExtraEvolutionNpcAssetQuery : IRequest<List<ExtraEvolutionNpcAssetDTO>>
    {
    }
}