using MediatR;
using DigitalWorldOnline.Commons.DTOs.Config;

namespace DigitalWorldOnline.Application.GameAssets.Queries
{
    public class FruitConfigsQuery : IRequest<List<FruitConfigDTO>>
    {
    }
}