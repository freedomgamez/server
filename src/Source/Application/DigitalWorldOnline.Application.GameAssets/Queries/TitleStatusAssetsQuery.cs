using MediatR;
using DigitalWorldOnline.Commons.DTOs.Assets;

namespace DigitalWorldOnline.Application.GameAssets.Queries
{
    public class TitleStatusAssetsQuery : IRequest<TitleStatusAssetDTO>
    {
        public short TitleId { get; set; }

        public TitleStatusAssetsQuery(short titleId)
        {
            TitleId = titleId;
        }
    }
}