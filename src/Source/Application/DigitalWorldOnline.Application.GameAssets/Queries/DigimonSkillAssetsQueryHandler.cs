using DigitalWorldOnline.Application.GameAssets.Bins;
using DigitalWorldOnline.Commons.DTOs.Assets;
using MediatR;

namespace DigitalWorldOnline.Application.GameAssets.Queries
{
    /// <summary>
    /// Backed by <c>Digimon_List.bin</c> instead of the DB.  The per-digimon skill loadout
    /// is the <c>s_Skill[4]</c> sub-array on each <c>CsDigimon::sINFO</c> record — slot
    /// index 0..3 maps directly to the client's F1..F4 hotbar (the client sends
    /// <c>slot=0</c> when F1 is pressed).  Server-side <c>Asset_DigimonSkill.Slot</c> was
    /// also 0-indexed pre-migration, so we keep the bin emit at 0..3 to match the client
    /// packet — anything else breaks `_assets.DigimonSkillInfo.FirstOrDefault(.. && Slot == skillSlot)`.
    /// Zero-id bin slots are skipped (unfilled hotbar positions on that digimon).
    /// </summary>
    public class DigimonSkillAssetsQueryHandler : IRequestHandler<DigimonSkillAssetsQuery, List<DigimonSkillAssetDTO>>
    {
        private readonly DigimonListBinLoader _loader;

        public DigimonSkillAssetsQueryHandler(DigimonListBinLoader loader)
        {
            _loader = loader;
        }

        public Task<List<DigimonSkillAssetDTO>> Handle(DigimonSkillAssetsQuery request, CancellationToken cancellationToken)
        {
            var list = new List<DigimonSkillAssetDTO>();
            long syntheticId = 0;
            foreach (var digimon in _loader.Data.ByType.Values)
            {
                for (int s = 0; s < digimon.Skills.Count; s++)
                {
                    var slot = digimon.Skills[s];
                    if (slot.SkillId == 0) continue;

                    list.Add(new DigimonSkillAssetDTO
                    {
                        Id = ++syntheticId,
                        Type = digimon.Type,
                        Slot = (byte)s,           // 0-indexed: matches client packet's skillSlot
                        SkillId = slot.SkillId,
                        // bin's s_nReqPrevSkillLevel — gate for the skill-up prereq check
                        // mirrored from client DigimonSkill.cpp:113.  Clamped to byte: client
                        // field is int but values are small (≤ MaxLevel ≤ 50).
                        RequiredLevel = (byte)System.Math.Clamp(slot.RequiredPrevSkillLevel, 0, byte.MaxValue),
                    });
                }
            }
            return Task.FromResult(list);
        }
    }
}
