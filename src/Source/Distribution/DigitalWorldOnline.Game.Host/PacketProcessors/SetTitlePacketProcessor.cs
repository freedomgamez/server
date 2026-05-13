using DigitalWorldOnline.Application;
using DigitalWorldOnline.Application.GameAssets;
using DigitalWorldOnline.Application.Separar.Commands.Update;
using DigitalWorldOnline.Commons.Entities;
using DigitalWorldOnline.Commons.Enums.PacketProcessor;
using DigitalWorldOnline.Commons.Interfaces;
using DigitalWorldOnline.Commons.Models;
using DigitalWorldOnline.Commons.Models.Character;
using DigitalWorldOnline.Commons.Models.Digimon;
using DigitalWorldOnline.Commons.Packets.GameServer;
using DigitalWorldOnline.Commons.Utils;
using DigitalWorldOnline.GameHost;


using MediatR;

namespace DigitalWorldOnline.Game.PacketProcessors
{
    public class SetTitlePacketProcessor : IGamePacketProcessor
    {
        public GameServerPacketEnum Type => GameServerPacketEnum.SetTitle;

        private readonly AssetsLoader _assets;
        private readonly MapServer _mapServer;
        private readonly ISender _sender;

        public SetTitlePacketProcessor(
            AssetsLoader assets,
            MapServer mapServer,
            ISender sender)
        {
            _assets = assets;
            _mapServer = mapServer;
            _sender = sender;
        }

        public async Task Process(GameClient client, byte[] packetData)
        {
            var packet = new GamePacketReader(packetData);

            var titleId = packet.ReadShort();

            var OldTitleBuff = _assets.AchievementAssets.FirstOrDefault(x => x.QuestId == client.Tamer.CurrentTitle && x.BuffId > 0);

            if (OldTitleBuff != null)
            {
                // Stash digimons (non-active partner) — used to drop the buff but never sent
                // a RemoveBuffPacket. The packet is self-only (the tamer is the only viewer
                // for stash digimons), but it still needs to fire so the client UI clears.
                foreach (var partner in client.Tamer.Digimons.Where(x => x.Id != client.Tamer.Partner.Id))
                {
                    if (partner.BuffList.ForceExpired(OldTitleBuff.BuffId))
                    {
                        partner.BuffList.Remove(OldTitleBuff.BuffId);
                        client?.Send(new RemoveBuffPacket(partner.GeneralHandler, OldTitleBuff.BuffId));

                        await _sender.Send(new UpdateDigimonBuffListCommand(partner.BuffList));
                    }
                }

                if (client.Partner.BuffList.ForceExpired(OldTitleBuff.BuffId))
                {
                    client.Partner.BuffList.Remove(OldTitleBuff.BuffId);
                    client?.Send(new RemoveBuffPacket(client.Partner.GeneralHandler, OldTitleBuff.BuffId));
                }

                // Tamer-side: title buffs whose SkillCode targets the tamer (e.g. cast-speed
                // bonuses) get applied to client.Tamer.BuffList below; they were never being
                // removed when the title changed, so the bonus persisted across title swaps.
                if (client.Tamer.BuffList.ForceExpired(OldTitleBuff.BuffId))
                {
                    client.Tamer.BuffList.Remove(OldTitleBuff.BuffId);
                    client?.Send(new RemoveBuffPacket(client.Tamer.GeneralHandler, OldTitleBuff.BuffId));
                }
            }

            var newTitle = _assets.AchievementAssets.FirstOrDefault(x => x.QuestId == titleId && x.BuffId > 0);

         

            if (newTitle != null)
            {
                var buff = _assets.BuffInfo.FirstOrDefault(x => x.BuffId == newTitle.BuffId);
                var newDigimonBuff = DigimonBuffModel.Create(buff.BuffId, buff.SkillId);

                newDigimonBuff.SetBuffInfo(buff);

                foreach (var partner in client.Tamer.Digimons.Where(x => x.Id != client.Tamer.Partner.Id))
                {
                    var partnernewDigimonBuff = DigimonBuffModel.Create(buff.BuffId, buff.SkillId);

                    partnernewDigimonBuff.SetBuffInfo(buff);

                    partner.BuffList.Add(partnernewDigimonBuff);

                        await _sender.Send(new UpdateDigimonBuffListCommand(partner.BuffList));                  
                }

                client.Partner.BuffList.Add(newDigimonBuff);
                _mapServer.BroadcastForTamerViewsAndSelf(client.TamerId,
                    new AddBuffPacket(client.Partner.GeneralHandler, buff, (short)0, 0).Serialize());

                // Tamer-side apply: mirrors the partner branch. Title buffs whose SkillCode
                // targets the tamer (DigimonSkillCode == 0 && SkillCode > 0) need to land on
                // the tamer's own buff list to actually take effect. The C2 overlap-reject
                // rule + BuffId dup-check prevents double-stacking if the buff isn't tamer-side.
                if (buff.DigimonSkillCode == 0 && buff.SkillCode > 0)
                {
                    var newCharacterBuff = CharacterBuffModel.Create(buff.BuffId, buff.SkillId);
                    newCharacterBuff.SetBuffInfo(buff);
                    client.Tamer.BuffList.Add(newCharacterBuff);
                    _mapServer.BroadcastForTamerViewsAndSelf(client.TamerId,
                        new AddBuffPacket(client.Tamer.GeneralHandler, buff, (short)0, 0).Serialize());
                }
            }

            client.Tamer.UpdateCurrentTitle(titleId);        

            //TODO: adicionar ao loader
//client.Partner?.SetTitleStatus(
//    _assets.TitleStatus.FirstOrDefault(x => x.ItemId == client.Tamer.CurrentTitle)
//);
//
            _mapServer.BroadcastForTamerViewsAndSelf(client.TamerId,
                new UpdateCurrentTitlePacket(client.Tamer.AppearenceHandler, titleId).Serialize());

            client.Send(new UpdateStatusPacket(client.Tamer));
            _mapServer.BroadcastForTamerViewsAndSelf(client.TamerId, new UpdateMovementSpeedPacket(client.Tamer).Serialize());

            await _sender.Send(new UpdateCharacterTitleCommand(client.TamerId, titleId));
            await _sender.Send(new UpdateDigimonBuffListCommand(client.Partner.BuffList));
        }
    }
}
