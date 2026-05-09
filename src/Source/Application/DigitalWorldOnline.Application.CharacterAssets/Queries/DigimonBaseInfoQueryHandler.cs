using DigitalWorldOnline.Application.CharacterAssets.Bins;
using DigitalWorldOnline.Commons.DTOs.Assets;
using DigitalWorldOnline.Commons.Enums.ClientEnums;
using MediatR;

namespace DigitalWorldOnline.Application.CharacterAssets.Queries
{
    /// <summary>
    /// Backed by <c>Digimon_List.bin</c> (the v487 single-section file of 634 ×
    /// <c>CsDigimon::sINFO</c>) instead of the DB. Bin field <c>s_eBaseNatureType</c> —
    /// the original DMO term — populates the server's <c>Element</c> column.
    /// </summary>
    public class DigimonBaseInfoQueryHandler : IRequestHandler<DigimonBaseInfoQuery, DigimonBaseInfoAssetDTO?>
    {
        private readonly DigimonListBinLoader _digimonList;

        public DigimonBaseInfoQueryHandler(DigimonListBinLoader digimonList)
        {
            _digimonList = digimonList;
        }

        public Task<DigimonBaseInfoAssetDTO?> Handle(DigimonBaseInfoQuery request, CancellationToken cancellationToken)
        {
            var entry = _digimonList.Data.FindByType(request.Type);
            if (entry == null)
                return Task.FromResult<DigimonBaseInfoAssetDTO?>(null);

            return Task.FromResult<DigimonBaseInfoAssetDTO?>(new DigimonBaseInfoAssetDTO
            {
                Type = entry.Type,
                Model = entry.Model,
                Name = entry.Name,
                Level = entry.BaseLevel,
                ScaleType = entry.ScaleType,
                EvolutionType = entry.EvolutionType,
                Attribute = (DigimonAttributeEnum)entry.Attribute,
                Element = (DigimonElementEnum)entry.Element,
                Family1 = (DigimonFamilyEnum)entry.Family1,
                Family2 = (DigimonFamilyEnum)entry.Family2,
                Family3 = (DigimonFamilyEnum)entry.Family3,

                HPValue = entry.HP,
                DSValue = entry.DS,
                DEValue = entry.DefPower,
                EVValue = entry.Evasion,
                MSValue = entry.MoveSpeed,
                CTValue = entry.CriticalRate,
                ATValue = entry.AttPower,
                ASValue = entry.AttSpeed,
                ARValue = entry.AttRange,
                HTValue = entry.HitRate
                // BL/WS not in CsDigimon::sINFO — left at default 0
            });
        }
    }
}
