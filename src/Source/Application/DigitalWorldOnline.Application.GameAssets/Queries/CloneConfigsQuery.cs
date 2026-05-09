using MediatR;
using DigitalWorldOnline.Commons.DTOs.Config;

namespace DigitalWorldOnline.Application.GameAssets.Queries
{
    public class CloneConfigsQuery : IRequest<List<CloneConfigDTO>>
    {
    }
}