using DigitalWorldOnline.Commons.Enums;
using MediatR;

namespace DigitalWorldOnline.Application.Separar.Commands.Create
{
    public class CreateCharacterItemListCommand : IRequest
    {
        public long CharacterId { get; }
        public ItemListEnum Type { get; }

        public CreateCharacterItemListCommand(long characterId, ItemListEnum type)
        {
            CharacterId = characterId;
            Type = type;
        }
    }
}
