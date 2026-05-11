using DigitalWorldOnline.Application.GameAssets.Bins;
using DigitalWorldOnline.Commons.DTOs.Assets;
using MediatR;

namespace DigitalWorldOnline.Application.GameAssets.Queries
{
    /// <summary>
    /// Backed by <c>Monster.bin</c> §3 (<c>CsMonsterSkill::sINFO</c>) instead of the DB.
    /// Emits one <see cref="MonsterSkillAssetDTO"/> per skill row — <c>Type</c> is the
    /// owning <c>s_dwMonsterID</c>, <c>SkillId</c> is the skill's <c>s_nSkill_IDX</c>
    /// (same value the join in <c>AssetsLoader.cs</c> uses to attach the info DTO).
    /// Replaces <c>_repository.GetMonsterSkillSkillAssetsAsync()</c>.
    /// </summary>
    public class MonsterSkillAssetsQueryHandler : IRequestHandler<MonsterSkillAssetsQuery, List<MonsterSkillAssetDTO>>
    {
        private readonly MonsterBinLoader _loader;

        public MonsterSkillAssetsQueryHandler(MonsterBinLoader loader)
        {
            _loader = loader;
        }

        public Task<List<MonsterSkillAssetDTO>> Handle(MonsterSkillAssetsQuery request, CancellationToken cancellationToken)
        {
            var list = new List<MonsterSkillAssetDTO>(_loader.Data.SkillsByIndex.Count);
            long syntheticId = 0;
            foreach (var skill in _loader.Data.SkillsByIndex.Values)
            {
                list.Add(new MonsterSkillAssetDTO
                {
                    Id = ++syntheticId,
                    Type = skill.MonsterType,
                    SkillId = skill.SkillIndex,
                });
            }
            return Task.FromResult(list);
        }
    }
}
