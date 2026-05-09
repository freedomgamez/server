using DigitalWorldOnline.Application.GameAssets.Bins;
using DigitalWorldOnline.Commons.DTOs.Assets;
using DigitalWorldOnline.Commons.Enums.ClientEnums;
using MediatR;

namespace DigitalWorldOnline.Application.GameAssets.Queries
{
    /// <summary>
    /// Backed by <c>Digimon_List.bin</c> (full 634 entries) instead of the DB.
    /// Bin's <c>s_eBaseNatureType</c> populates the DTO's <c>Element</c>.
    /// </summary>
    public class AllDigimonBaseInfoQueryHandler : IRequestHandler<AllDigimonBaseInfoQuery, IList<DigimonBaseInfoAssetDTO>>
    {
        private readonly DigimonListBinLoader _digimonList;

        public AllDigimonBaseInfoQueryHandler(DigimonListBinLoader digimonList)
        {
            _digimonList = digimonList;
        }

        public Task<IList<DigimonBaseInfoAssetDTO>> Handle(AllDigimonBaseInfoQuery request, CancellationToken cancellationToken)
        {
            var data = _digimonList.Data.ByType;
            var list = new List<DigimonBaseInfoAssetDTO>(data.Count);
            foreach (var entry in data.Values)
            {
                list.Add(new DigimonBaseInfoAssetDTO
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
                });
            }
            return Task.FromResult<IList<DigimonBaseInfoAssetDTO>>(list);
        }
    }
}
