using DigitalWorldOnline.Commons.Enums.ClientEnums;
using DigitalWorldOnline.Commons.Models.Character;
using DigitalWorldOnline.Commons.Writers;

namespace DigitalWorldOnline.Commons.Packets.CharacterServer
{
    public class CharacterListPacket : PacketWriter
    {
        private const int PacketNumber = 1301;

        public CharacterListPacket(IEnumerable<CharacterModel> characters)
        {
            Type(PacketNumber);

            foreach (var character in characters.Where(x => x.Partner != null))
            {
                WriteByte(character.Position);
                WriteShort(character.Location.MapId);
                WriteInt(character.Model.GetHashCode());
                WriteByte(character.Level);
                WriteString(character.Name);

                for (int i = 0; i < GeneralSizeEnum.Equipment.GetHashCode(); i++)
                {
                    if(i == 11)
                        WriteBytes(character.Digivice.Items[0].ToArray(true));
                    else
                        WriteBytes(character.Equipment.Items[i].ToArray(true));
                }

                WriteInt(character.Partner.BaseType);
                WriteByte(character.Partner.Level);
                WriteString(character.Partner.Name);
                WriteShort(character.Partner.Size);

                // Keep legacy 6-byte tail for this client build:
                // u2 patLeader + n4 reserved/relocate.
                // (Do NOT send digimonEffectType here unless the client parser
                // is built with the matching partssystem layout.)
                WriteShort(character.SealList.SealLeaderId);
                WriteInt(0);
            }

            WriteByte(99);
        }
    }
}
