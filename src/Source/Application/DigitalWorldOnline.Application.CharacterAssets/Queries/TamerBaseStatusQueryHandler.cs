using DigitalWorldOnline.Application.CharacterAssets.Bins;
using DigitalWorldOnline.Commons.DTOs.Assets;
using DigitalWorldOnline.Commons.Enums;
using MediatR;

namespace DigitalWorldOnline.Application.CharacterAssets.Queries
{
    /// <summary>
    /// Backed by <c>DMBase.bin</c> section 1 (per-tamer per-level stat table) instead of the DB.
    /// Only level-1 rows are kept in memory (12 of them, one per tamer model) since this handler
    /// is only consulted at character creation. The 8 stat fields the bin carries
    /// (HP/DS/MS/DE/EV/CT/AT/HT) populate the matching <see cref="Commons.DTOs.Base.StatusDTO"/>
    /// fields; AS/AR/BL/WS are not in DMBase.bin and are left at their default 0 — tamer-side
    /// AS/AR/BL/WS are not per-level scaling inputs in the v487 client.
    /// </summary>
    public class TamerBaseStatusQueryHandler : IRequestHandler<TamerBaseStatusQuery, CharacterBaseStatusAssetDTO>
    {
        private readonly DMBaseBinLoader _dmBase;

        public TamerBaseStatusQueryHandler(DMBaseBinLoader dmBase)
        {
            _dmBase = dmBase;
        }

        public Task<CharacterBaseStatusAssetDTO> Handle(TamerBaseStatusQuery request, CancellationToken cancellationToken)
        {
            int tamerModel = (int)request.Type;
            var rec = _dmBase.Data.FindTamerStats(tamerModel, level: 1)
                ?? throw new InvalidOperationException(
                    $"DMBase.bin section 1 has no level-1 stats for tamer model {tamerModel}.");

            return Task.FromResult(new CharacterBaseStatusAssetDTO
            {
                Type = (CharacterModelEnum)tamerModel,
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
    }
}
