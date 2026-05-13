using DigitalWorldOnline.Application;
using DigitalWorldOnline.Application.GameAssets;
using DigitalWorldOnline.Application.Separar.Commands.Update;
using DigitalWorldOnline.Commons.Entities;
using DigitalWorldOnline.Commons.Enums;
using DigitalWorldOnline.Commons.Enums.ClientEnums;
using DigitalWorldOnline.Commons.Enums.PacketProcessor;
using DigitalWorldOnline.Commons.Interfaces;
using DigitalWorldOnline.Commons.Models.Base;
using DigitalWorldOnline.Commons.Packets.Chat;
using DigitalWorldOnline.Commons.Packets.GameServer;
using DigitalWorldOnline.Commons.Packets.Items;
using DigitalWorldOnline.Commons.Utils;
using DigitalWorldOnline.GameHost;
using MediatR;
using Serilog;


namespace DigitalWorldOnline.Game.PacketProcessors
{
    public class ItemSocketInPacketProcessor : IGamePacketProcessor
    {
        public GameServerPacketEnum Type => GameServerPacketEnum.ItemSocketIn;
        private readonly ILogger _logger;
        private readonly ISender _sender;

        public ItemSocketInPacketProcessor(
            ILogger logger,
            ISender sender)
        {
            _logger = logger;
            _sender = sender;

        }

        public async Task Process(GameClient client, byte[] packetData)
        {
            var packet = new GamePacketReader(packetData);

            _ = packet.ReadInt();
            int remaining = (packet.Length - 2) - (int)packet.Packet.Position;
            short sourceSlot;
            short destinationSlot;

            if (remaining == 9)
            {
                _ = packet.ReadInt(); // npcId
                sourceSlot = packet.ReadShort();
                destinationSlot = packet.ReadShort();
                _ = packet.ReadByte(); // socketOrder
            }
            else if (remaining == 10)
            {
                _ = packet.ReadByte(); // vip
                _ = packet.ReadInt();  // npcId
                sourceSlot = packet.ReadShort();
                destinationSlot = packet.ReadShort();
                _ = packet.ReadByte(); // socketOrder
            }
            else if (remaining == 13)
            {
                _ = packet.ReadInt();  // portableSlot
                _ = packet.ReadInt();  // npcId
                sourceSlot = packet.ReadShort();
                destinationSlot = packet.ReadShort();
                _ = packet.ReadByte(); // socketOrder
            }
            else
            {
                _ = packet.ReadByte(); // vip
                _ = packet.ReadInt();  // portableSlot
                _ = packet.ReadInt();  // npcId
                sourceSlot = packet.ReadShort();
                destinationSlot = packet.ReadShort();
                _ = packet.ReadByte(); // socketOrder
            }

            var itemInfo = client.Tamer.Inventory.FindItemBySlot(sourceSlot);
            var destinationInfo = client.Tamer.Inventory.FindItemBySlot(destinationSlot);

            if (itemInfo != null && destinationInfo != null)
            {
                var avaliableSocket = destinationInfo.SocketStatus.FirstOrDefault(x => x.AttributeId == 0);
                var avaliableStatus = destinationInfo.AccessoryStatus.FirstOrDefault(x => x.Value == 0);

                if (avaliableSocket != null && avaliableStatus != null)
                {
                    var attributeApply = itemInfo.AccessoryStatus.FirstOrDefault(x => x.Value > 0);
                    if (attributeApply == null)
                    {
                        client.Send(UtilitiesFunctions.GroupPackets(
                            new ItemSocketInPacket((int)client.Tamer.Inventory.Bits).Serialize(),
                            new SystemMessagePacket("Invalid source socket item.").Serialize(),
                            new LoadInventoryPacket(client.Tamer.Inventory, InventoryTypeEnum.Inventory).Serialize()));
                        return;
                    }

                    destinationInfo.SetPower(itemInfo.Power);
                    destinationInfo.SetReroll(0);
                    avaliableSocket.SetType(attributeApply.Type);
                    avaliableSocket.SetAttributeId((short)itemInfo.ItemId);
                    avaliableSocket.SetValue(itemInfo.Power);
                    avaliableStatus.SetType(attributeApply.Type);
                    avaliableStatus.SetValue(attributeApply.Value);
                }

                client.Tamer.Inventory.RemoveOrReduceItem(itemInfo, 1, sourceSlot);
                client.Tamer.Inventory.RemoveBits(itemInfo.ItemInfo.ScanPrice / 2);

                client.Send(UtilitiesFunctions.GroupPackets(
                    new ItemSocketInPacket((int)client.Tamer.Inventory.Bits).Serialize(),
                    new LoadInventoryPacket(client.Tamer.Inventory, InventoryTypeEnum.Inventory).Serialize()));

                await _sender.Send(new UpdateItemSocketStatusCommand(destinationInfo));
                await _sender.Send(new UpdateItemAccessoryStatusCommand(destinationInfo));
                await _sender.Send(new UpdateItemsCommand(client.Tamer.Inventory));
                await _sender.Send(new UpdateItemListBitsCommand(client.Tamer.Inventory));
            }
            else
            {
                client.Send(UtilitiesFunctions.GroupPackets(
                    new ItemSocketInPacket((int)client.Tamer.Inventory.Bits).Serialize(),
                    new PickItemFailPacket(PickItemFailReasonEnum.InventoryFull).Serialize(),
                    new LoadInventoryPacket(client.Tamer.Inventory, InventoryTypeEnum.Inventory).Serialize()));
            }

        }


        public int GetSkillAtt(ItemModel item, int nSkillLevel, int nApplyIndex)
        {


            var Skill = item.ItemInfo.SkillInfo;

            bool bDigimonSkill = IsDigimonSkill((int)item.ItemInfo.SkillCode);

            if (!bDigimonSkill ||   // 디지몬 스킬이 아닌경우(아이템/테이머) 와
                nApplyIndex == 0)   // 디지몬 공격스킬 효과는 기존 공식 사용
                return Skill.Apply[nApplyIndex].Value + nSkillLevel * Skill.Apply[nApplyIndex].IncreaseValue;

            //디지몬 특수스킬 효과는 레벨을 1 빼주고 계산
            switch ((ApplyStatusEnum)Skill.Apply[nApplyIndex].Attribute)
            {
                // 크리티컬 / 회피 증가 스킬 추가 14.05.28 chu8820
                case ApplyStatusEnum.APPLY_CA:
                case ApplyStatusEnum.APPLY_EV:
                    {
                        if (Skill.Apply[nApplyIndex].Type == SkillCodeApplyTypeEnum.Unknown207)
                        {
                            int nValue = Skill.Apply[nApplyIndex].Value + (nSkillLevel - 1) * Skill.Apply[nApplyIndex].IncreaseValue;
                            return (nValue * 100);
                        }
                        else //if( pFTSkill->s_Apply[ nApplyIndex ].s_nID == 206/*nSkill::Me_206*/ )
                            return Skill.Apply[nApplyIndex].Value + (nSkillLevel - 1) * Skill.Apply[nApplyIndex].IncreaseValue;
                    }
                default:
                    return Skill.Apply[nApplyIndex].Value + (nSkillLevel - 1) * Skill.Apply[nApplyIndex].IncreaseValue;
            }

            return Skill.Apply[nApplyIndex].Value + nSkillLevel * Skill.Apply[nApplyIndex].IncreaseValue;
        }

        public bool IsDigimonSkill(int SkillId)
        {
            if (SkillId / 100 == 21134 || SkillId / 100 == 21137)// 페티메라몬, 시드몬 예외처리
            {
                return true;
            }
            else
            {
                return (SkillId / 1000000 >= 3 && SkillId / 1000000 <= 7);
            }


        }
    }
}
