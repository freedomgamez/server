using MediatR;
using DigitalWorldOnline.Commons.DTOs.Assets;

namespace DigitalWorldOnline.Application.GameAssets.Queries
{
    public class MonthlyEventAssetsQuery : IRequest<List<MonthlyEventAssetDTO>>
    {
    }
}