using DigitalWorldOnline.Application.GameAssets.Bins;
using DigitalWorldOnline.Commons.DTOs.Assets;
using DigitalWorldOnline.Commons.Enums.ClientEnums;
using MediatR;

namespace DigitalWorldOnline.Application.GameAssets.Queries
{
    /// <summary>
    /// Backed by <c>DigimonEvo.bin</c> (full 140 trees) plus <c>Digimon_List.bin</c> for
    /// the per-base-digimon <c>EvolutionRank</c>. Server's <c>DigimonModel.AddEvolutions</c>
    /// only reads <c>Lines[].Type</c>; the other DTO fields are populated for completeness.
    /// </summary>
    public class DigimonEvolutionAssetsQueryHandler : IRequestHandler<DigimonEvolutionAssetsQuery, List<EvolutionAssetDTO>>
    {
        private readonly DigimonEvoBinLoader _digimonEvo;
        private readonly DigimonListBinLoader _digimonList;
        private readonly DMBaseBinLoader _dmBase;

        public DigimonEvolutionAssetsQueryHandler(
            DigimonEvoBinLoader digimonEvo,
            DigimonListBinLoader digimonList,
            DMBaseBinLoader dmBase)
        {
            _digimonEvo = digimonEvo;
            _digimonList = digimonList;
            _dmBase = dmBase;
        }

        public Task<List<EvolutionAssetDTO>> Handle(DigimonEvolutionAssetsQuery request, CancellationToken cancellationToken)
        {
            var trees = _digimonEvo.Data.ByType;
            var result = new List<EvolutionAssetDTO>(trees.Count);

            foreach (var tree in trees.Values)
            {
                var listEntry = _digimonList.Data.FindByType(tree.Type);
                var rank = listEntry != null
                    ? (EvolutionRankEnum)listEntry.EvolutionType
                    : EvolutionRankEnum.None;

                var lines = new List<EvolutionLineAssetDTO>(tree.Lines.Count);
                foreach (var line in tree.Lines)
                {
                    lines.Add(new EvolutionLineAssetDTO
                    {
                        Type = line.Type,
                        SlotLevel = (byte)Math.Min(line.EvoSlot, byte.MaxValue),
                        UnlockLevel = (byte)Math.Min(line.OpenLevel, byte.MaxValue),
                        UnlockQuestId = (short)line.OpenQuest,
                        UnlockItemSection = line.UseItem,
                        UnlockItemSectionAmount = line.UseItemNum,
                        SkillMaxLevels = ResolveSkillMaxLevels(line.Type),
                        Stages = line.Stages
                            .Select(s => new EvolutionStageAssetDTO { Type = s.TargetType, Value = s.Slot })
                            .ToList()
                    });
                }

                result.Add(new EvolutionAssetDTO
                {
                    Type = tree.Type,
                    EvolutionRank = rank,
                    Lines = lines
                });
            }

            return Task.FromResult(result);
        }

        /// <summary>
        /// Look up per-skill-slot max levels for the given evolution form by:
        /// 1. Resolving the form's evolution stage from <c>Digimon_List.bin</c> (its <c>EvolutionType</c> field).
        /// 2. Indexing into <c>DMBase.bin</c> section 12 (<c>DigimonEvoMaxLevel</c>) by that stage.
        /// Returns <c>null</c> if either lookup fails — caller falls back to model defaults.
        /// </summary>
        private byte[]? ResolveSkillMaxLevels(int digimonType)
        {
            var listEntry = _digimonList.Data.FindByType(digimonType);
            if (listEntry == null) return null;

            if (!_dmBase.Data.DigimonEvoMaxLevel.TryGetValue(listEntry.EvolutionType, out var caps))
                return null;

            var arr = new byte[caps.SkillMaxLevels.Count];
            for (int i = 0; i < arr.Length; i++)
                arr[i] = (byte)Math.Min(caps.SkillMaxLevels[i], byte.MaxValue);
            return arr;
        }
    }
}
