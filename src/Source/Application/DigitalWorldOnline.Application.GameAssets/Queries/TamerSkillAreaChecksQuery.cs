using DigitalWorldOnline.Commons.DTOs.Assets;
using MediatR;

namespace DigitalWorldOnline.Application.GameAssets.Queries
{
    public sealed class TamerSkillAreaChecksQuery : IRequest<List<TamerSkillAreaCheckAssetDTO>>
    {
    }
}
