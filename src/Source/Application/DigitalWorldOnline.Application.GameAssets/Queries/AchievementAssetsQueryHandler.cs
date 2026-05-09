using DigitalWorldOnline.Application.GameAssets.Bins;
using DigitalWorldOnline.Commons.Models.Asset;
using MediatR;

namespace DigitalWorldOnline.Application.GameAssets.Queries
{
    /// <summary>
    /// Backed by <c>Achieve.bin</c> §B (the per-achievement detail records). The bin's
    /// per-record fields map directly to <c>AchievementAssetDTO</c>: <c>QuestId</c> is the
    /// join key against <c>Quest.bin</c>, <c>BuffId</c> is non-zero only for achievements
    /// that grant a title (the buff drives the title's effect — see
    /// <c>SetTitlePacketProcessor.cs</c>). The bin's <c>QuestID</c>/<c>Type</c> are stored
    /// as DWORD/int respectively but the server's DTO narrows to <c>short</c>/<c>byte</c>;
    /// v487 has no values exceeding those ranges in practice (max QuestID seen ≈ 6500,
    /// Type ∈ 0..2).
    /// </summary>
    public class AchievementAssetsQueryHandler : IRequestHandler<AchievementAssetsQuery, List<AchievementAssetDTO>>
    {
        private readonly AchieveBinLoader _loader;

        public AchievementAssetsQueryHandler(AchieveBinLoader loader)
        {
            _loader = loader;
        }

        public Task<List<AchievementAssetDTO>> Handle(AchievementAssetsQuery request, CancellationToken cancellationToken)
        {
            var list = new List<AchievementAssetDTO>(_loader.Data.All.Count);
            foreach (var rec in _loader.Data.All)
            {
                list.Add(new AchievementAssetDTO
                {
                    QuestId = (short)rec.QuestId,
                    Type = (byte)rec.Type,
                    BuffId = rec.BuffCode
                });
            }
            return Task.FromResult(list);
        }
    }
}
