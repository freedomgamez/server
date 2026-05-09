using DigitalWorldOnline.Application;
using DigitalWorldOnline.Application.GameAssets;
using DigitalWorldOnline.Application.GameAssets.Bins;
using DigitalWorldOnline.Application.Separar.Commands.Update;
using DigitalWorldOnline.Commons.Entities;
using DigitalWorldOnline.Commons.Enums.PacketProcessor;
using DigitalWorldOnline.Commons.Interfaces;
using DigitalWorldOnline.Commons.Packets.GameServer;
using MediatR;
using Serilog;

namespace DigitalWorldOnline.Game.PacketProcessors
{
    /// <summary>
    /// Handler for <c>pDigimon::DigimonSkillLimitOpen</c> (3245) — D-skill expansion.
    ///
    /// Wire layout from <c>C2GS_SEND_DIGIMON_SKILL_LIMIT_OPEN</c>:
    ///   u4 nItemPos          (inventory slot)
    ///   u4 nItemType         (item type id)
    ///   u4 nDigimonEvoArray  (evolution slot index)
    ///
    /// Validation pulls from DMBase.bin §13 (DskillOpenExpansion) — keyed by item.Section,
    /// returns ExpansionRank + AllowedEvoTypes. The bin doesn't carry the rank → cap-delta
    /// mapping; we use a simple convention (rank 1/2/3 → +5/+10/+15 per skill) which matches
    /// typical Korean MMO expansion-item conventions. Adjust here if better data appears.
    ///
    /// On success: bumps every skill slot's MaxLevel by the rank delta, persists the evolution,
    /// decrements the item, and replies with the updated cEvoUnit. On failure: replies with a
    /// non-zero result code so the client shows the appropriate cPrintMsg.
    /// </summary>
    public class DigimonSkillLimitOpenPacketProcessor : IGamePacketProcessor
    {
        public GameServerPacketEnum Type => GameServerPacketEnum.DigimonSkillLimitOpen;

        private readonly AssetsLoader _assets;
        private readonly DMBaseBinLoader _dmBase;
        private readonly DigimonListBinLoader _digimonList;
        private readonly ISender _sender;
        private readonly ILogger _logger;

        public DigimonSkillLimitOpenPacketProcessor(
            AssetsLoader assets,
            DMBaseBinLoader dmBase,
            DigimonListBinLoader digimonList,
            ISender sender,
            ILogger logger)
        {
            _assets = assets;
            _dmBase = dmBase;
            _digimonList = digimonList;
            _sender = sender;
            _logger = logger;
        }

        public async Task Process(GameClient client, byte[] packetData)
        {
            var reader = new GamePacketReader(packetData);
            int itemPos = reader.ReadInt();
            int itemType = reader.ReadInt();
            int evoArray = reader.ReadInt();

            // 1. Locate the item in inventory and verify it's the claimed type.
            var inv = client.Tamer.Inventory;
            var item = (itemPos >= 0 && itemPos < inv.Items.Count) ? inv.Items[itemPos] : null;
            if (item == null || item.ItemId == 0 || item.ItemId != itemType)
            {
                _logger.Warning("Tamer {TamerId}: D-skill expansion item mismatch (pos={Pos}, type={Type}).",
                    client.TamerId, itemPos, itemType);
                client.Send(new DigimonSkillLimitOpenResultPacket(SkillLimitOpenResult.NoItem, evoArray, null, itemPos, itemType));
                return;
            }

            var itemInfo = _assets.ItemInfo.FirstOrDefault(x => x.ItemId == item.ItemId);
            if (itemInfo == null || itemInfo.Type != 202)
            {
                _logger.Warning("Tamer {TamerId}: item {ItemId} is not a Type-202 D-skill expansion item.",
                    client.TamerId, item.ItemId);
                client.Send(new DigimonSkillLimitOpenResultPacket(SkillLimitOpenResult.NoItem, evoArray, null, itemPos, itemType));
                return;
            }

            // 2. Look up bin entry (section 13).
            if (!_dmBase.Data.DskillOpenExpansion.TryGetValue(itemInfo.Section, out var cfg))
            {
                _logger.Warning("Tamer {TamerId}: item {ItemId} section {Sec} not in DMBase §13.",
                    client.TamerId, item.ItemId, itemInfo.Section);
                client.Send(new DigimonSkillLimitOpenResultPacket(SkillLimitOpenResult.InternalError, evoArray, null, itemPos, itemType));
                return;
            }

            // 3. Look up the partner's evolution at the specified slot.
            var partner = client.Partner;
            if (partner == null || evoArray < 0 || evoArray >= partner.Evolutions.Count)
            {
                client.Send(new DigimonSkillLimitOpenResultPacket(SkillLimitOpenResult.InvalidEvolution, evoArray, null, itemPos, itemType));
                return;
            }
            var evo = partner.Evolutions[evoArray];

            // 4. Validate the target evolution's stage is in the bin allowlist.
            var listEntry = _digimonList.Data.FindByType(evo.Type);
            int evoStage = listEntry?.EvolutionType ?? 0;
            if (!cfg.AllowedEvoTypes.Contains(evoStage))
            {
                _logger.Information("Tamer {TamerId}: D-skill rank {Rank} not allowed on evo type {Type} (stage {Stage}).",
                    client.TamerId, cfg.ExpansionRank, evo.Type, evoStage);
                client.Send(new DigimonSkillLimitOpenResultPacket(SkillLimitOpenResult.StageNotAllowed, evoArray, null, itemPos, itemType));
                return;
            }

            // 5. Apply the cap-raise. Convention: rank N raises every skill slot's cap by 5*N.
            //    The bin doesn't encode this delta; if a different scaling is desired (e.g.,
            //    adding to a single skill slot rather than all five), revise here.
            byte capDelta = (byte)(cfg.ExpansionRank * 5);
            bool anyRaised = false;
            foreach (var skill in evo.Skills)
            {
                int newCap = skill.MaxLevel + capDelta;
                if (newCap > byte.MaxValue) newCap = byte.MaxValue;
                if (newCap > skill.MaxLevel)
                {
                    skill.SetMaxLevel((byte)newCap);
                    anyRaised = true;
                }
            }

            if (!anyRaised)
            {
                client.Send(new DigimonSkillLimitOpenResultPacket(SkillLimitOpenResult.AlreadyMaxed, evoArray, evo, itemPos, itemType));
                return;
            }

            // 6. Persist evolution + decrement item.
            await _sender.Send(new UpdateEvolutionCommand(evo));
            inv.RemoveOrReduceItem(item, 1, itemPos);
            await _sender.Send(new UpdateItemsCommand(inv));

            _logger.Information(
                "Tamer {TamerId}: D-skill expansion success — partner {PartnerId} evo-slot {Slot} (type {Type}) caps raised by {Delta} (rank {Rank}).",
                client.TamerId, partner.Id, evoArray, evo.Type, capDelta, cfg.ExpansionRank);

            // 7. Reply with the updated cEvoUnit so the client refreshes its in-memory state.
            client.Send(new DigimonSkillLimitOpenResultPacket(SkillLimitOpenResult.Success, evoArray, evo, itemPos, itemType));
        }
    }
}
