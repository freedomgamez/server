using DigitalWorldOnline.Application.GameAssets.Bins;
using DigitalWorldOnline.Commons.DTOs.Assets;
using DigitalWorldOnline.Commons.Enums;
using MediatR;

namespace DigitalWorldOnline.Application.GameAssets.Queries
{
    /// <summary>
    /// Backed by <c>Skill.bin §1 CsSkill</c> instead of the DB.  Each <c>SkillRecord</c>
    /// emits a <c>SkillCodeAssetDTO</c> with three <c>SkillCodeApplyAssetDTO</c> children
    /// (the <c>s_Apply[3]</c> sub-array on disk).  String fields (<c>Comment</c>) are
    /// skipped per the bin loader's no-strings convention; downstream consumers don't read it.
    /// </summary>
    public class SkillCodeAssetsQueryHandler : IRequestHandler<SkillCodeAssetsQuery, List<SkillCodeAssetDTO>>
    {
        private readonly SkillBinLoader _loader;

        public SkillCodeAssetsQueryHandler(SkillBinLoader loader)
        {
            _loader = loader;
        }

        public Task<List<SkillCodeAssetDTO>> Handle(SkillCodeAssetsQuery request, CancellationToken cancellationToken)
        {
            var list = new List<SkillCodeAssetDTO>(_loader.Data.ById.Count);
            foreach (var rec in _loader.Data.ById.Values)
            {
                var dto = new SkillCodeAssetDTO
                {
                    Id = rec.Id,
                    SkillCode = rec.Id,
                    Comment = string.Empty,
                    Apply = new List<SkillCodeApplyAssetDTO>(3)
                };
                dto.Apply.Add(MapApply(rec.Apply0, rec.Id));
                dto.Apply.Add(MapApply(rec.Apply1, rec.Id));
                dto.Apply.Add(MapApply(rec.Apply2, rec.Id));
                list.Add(dto);
            }
            return Task.FromResult(list);
        }

        private static SkillCodeApplyAssetDTO MapApply(SkillApply apply, int parentSkillId) => new()
        {
            Id = 0,
            Type = (SkillCodeApplyTypeEnum)apply.Id,
            Attribute = (SkillCodeApplyAttributeEnum)apply.A,
            Value = apply.B,
            Chance = apply.InvokeRate,
            AdditionalValue = apply.C,
            IncreaseValue = apply.IncreaseBPoint,
            SkillCodeAssetId = parentSkillId
        };
    }
}
