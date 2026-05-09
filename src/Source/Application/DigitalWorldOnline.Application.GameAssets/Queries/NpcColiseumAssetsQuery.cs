using MediatR;
using DigitalWorldOnline.Commons.DTOs.Assets;

namespace DigitalWorldOnline.Application.GameAssets.Queries
{
    public class NpcColiseumAssetsQuery : IRequest<List<NpcColiseumAssetDTO>>
    {
    }
}