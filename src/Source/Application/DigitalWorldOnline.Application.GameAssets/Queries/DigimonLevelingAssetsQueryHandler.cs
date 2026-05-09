using DigitalWorldOnline.Application.GameAssets.Bins;
using DigitalWorldOnline.Commons.DTOs.Assets;
using MediatR;

namespace DigitalWorldOnline.Application.GameAssets.Queries
{
    /// <summary>
    /// Backed by <c>DMBase.bin</c> section 2 (480 rows = 4 digimon ranks × 120 levels) instead
    /// of the DB. The bin's <c>s_dwID</c> encodes <c>digimonRank * 1000 + level</c>; we expose
    /// the rank in the DTO's <c>Type</c> field — Game.Host's level-up math joins it with the
    /// per-digimon base info from <c>Digimon_List.bin</c> (which carries the rank) at runtime.
    /// </summary>
    public class DigimonLevelingAssetsQueryHandler : IRequestHandler<DigimonLevelingAssetsQuery, List<DigimonLevelStatusAssetDTO>>
    {
        private readonly DMBaseBinLoader _dmBase;

        public DigimonLevelingAssetsQueryHandler(DMBaseBinLoader dmBase)
        {
            _dmBase = dmBase;
        }

        public Task<List<DigimonLevelStatusAssetDTO>> Handle(DigimonLevelingAssetsQuery request, CancellationToken cancellationToken)
        {
            var rows = _dmBase.Data.DigimonStats;
            var list = new List<DigimonLevelStatusAssetDTO>(rows.Count);
            foreach (var rec in rows.Values)
            {
                int rank = rec.Id / 1000;
                list.Add(new DigimonLevelStatusAssetDTO
                {
                    Type = rank,
                    // ScaleType is queried by ExpManager (`x.ScaleType == digimon.BaseInfo.ScaleType`)
                    // and equals the digimon rank (1..4) in v487 — see DigimonListBinLoader for
                    // why ScaleType ≡ s_nDigimonType, not s_dwCharSize.
                    ScaleType = rank,
                    // StatusId is queried by StatusManager.GetDigimonBaseStatus
                    // (`x.StatusId == level + ScaleType * 1000`) which equals the bin's record key.
                    // Without this, .Single() throws and InitialInformationPacketProcessor hangs
                    // (player loading screen stuck after character select).
                    StatusId = rec.Id,
                    Level = (byte)rec.Level,
                    // Bin's ExpValue is in WIRE units (100× the real threshold) — same
                    // convention as TamerLevelingAssetsQueryHandler. Divide here so the
                    // server's level-up comparison against `digimon.CurrentExperience`
                    // (stored real-units) matches DB-era behavior.
                    ExpValue = rec.Exp / 100,
                    HPValue = rec.HP,
                    DSValue = rec.DS,
                    MSValue = rec.MoveSpeed,
                    DEValue = rec.Defence,
                    EVValue = rec.Evasion,
                    CTValue = rec.Critical,
                    ATValue = rec.Attack,
                    HTValue = rec.HitRate
                });
            }
            return Task.FromResult(list);
        }
    }
}
