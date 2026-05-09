using DigitalWorldOnline.Commons.DTOs.Assets;
using DigitalWorldOnline.Commons.Interfaces;
using MediatR;

namespace DigitalWorldOnline.Application.GameAssets.Queries
{
    public class TamerBaseStatusAssetsQueryHandler : IRequestHandler<TamerBaseStatusAssetsQuery, List<CharacterBaseStatusAssetDTO>>
    {
        private readonly IServerQueriesRepository _repository;

        public TamerBaseStatusAssetsQueryHandler(IServerQueriesRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<CharacterBaseStatusAssetDTO>> Handle(TamerBaseStatusAssetsQuery request, CancellationToken cancellationToken)
        {
            // Retired — see GameAssets/AssetsLoader.cs (TamerBaseInfo). Returning empty list keeps
            // any orphaned consumer well-formed; no caller is wired now.
            return await Task.FromResult(new List<CharacterBaseStatusAssetDTO>());
            //return await _repository.GetAllTamerBaseStatusAsync();
        }
    }
}