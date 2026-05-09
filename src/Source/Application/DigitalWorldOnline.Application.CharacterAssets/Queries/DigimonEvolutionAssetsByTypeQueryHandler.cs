using DigitalWorldOnline.Application.CharacterAssets.Bins;
using DigitalWorldOnline.Commons.DTOs.Assets;
using DigitalWorldOnline.Commons.Enums.ClientEnums;
using MediatR;

namespace DigitalWorldOnline.Application.CharacterAssets.Queries
{
    /// <summary>
    /// Backed by <c>DigimonEvo.bin</c> (filtered to selectable starter types) instead of the DB.
    /// The server's <c>DigimonModel.AddEvolutions</c> only reads <c>Lines[].Type</c> — it
    /// auto-unlocks the first 3 entries — so we hand back a Lines list ordered by EvoSlot,
    /// which gives the natural Rookie→Champion→Ultimate progression for the auto-unlock window.
    /// </summary>
    public class DigimonEvolutionAssetsByTypeQueryHandler : IRequestHandler<DigimonEvolutionAssetsByTypeQuery, EvolutionAssetDTO>
    {
        private readonly DigimonEvoBinLoader _digimonEvo;
        private readonly DigimonListBinLoader _digimonList;

        public DigimonEvolutionAssetsByTypeQueryHandler(
            DigimonEvoBinLoader digimonEvo,
            DigimonListBinLoader digimonList)
        {
            _digimonEvo = digimonEvo;
            _digimonList = digimonList;
        }

        public Task<EvolutionAssetDTO> Handle(DigimonEvolutionAssetsByTypeQuery request, CancellationToken cancellationToken)
        {
            var entry = _digimonEvo.Data.FindByType(request.Type)
                ?? throw new InvalidOperationException(
                    $"DigimonEvo.bin has no evolution entry for digimon type {request.Type}. " +
                    "If this is a non-starter type, the loader's filter is excluding it; either " +
                    "expand the filter or pre-resolve the type before calling this query.");

            // EvolutionRank is the base digimon's tier (Rookie/Champion/etc). Cross-look it up
            // from Digimon_List.bin's evolutionType field — no need to embed it in DigimonEvo.bin.
            var listEntry = _digimonList.Data.FindByType(request.Type);
            var rank = listEntry != null
                ? (EvolutionRankEnum)listEntry.EvolutionType
                : EvolutionRankEnum.None;

            var lines = new List<EvolutionLineAssetDTO>(entry.Lines.Count);
            foreach (var line in entry.Lines)
            {
                lines.Add(new EvolutionLineAssetDTO
                {
                    Type = line.Type,
                    UnlockLevel = (byte)Math.Min(line.OpenLevel, byte.MaxValue),
                    UnlockQuestId = (short)line.OpenQuest
                });
            }

            return Task.FromResult(new EvolutionAssetDTO
            {
                Type = entry.Type,
                EvolutionRank = rank,
                Lines = lines
            });
        }
    }
}
