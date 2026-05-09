using MediatR;
using DigitalWorldOnline.Commons.DTOs.Assets;

namespace DigitalWorldOnline.Application.CharacterAssets.Queries
{
    public class DigimonEvolutionAssetsByTypeQuery : IRequest<EvolutionAssetDTO>
    {
        public int Type { get; }

        public DigimonEvolutionAssetsByTypeQuery(int type)
        {
            Type = type;
        }
    }
}
