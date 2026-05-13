using DigitalWorldOnline.Commons.Enums;
using DigitalWorldOnline.Commons.Interfaces;
using MediatR;

namespace DigitalWorldOnline.Application.Separar.Commands.Create
{
    public class CreateCharacterCommandHandler : IRequestHandler<CreateCharacterCommand, long>
    {
        private readonly ICharacterCommandsRepository _repository;
        private readonly ISender _sender;

        public CreateCharacterCommandHandler(ICharacterCommandsRepository repository, ISender sender)
        {
            _repository = repository;
            _sender = sender;
        }

        public async Task<long> Handle(CreateCharacterCommand request, CancellationToken cancellationToken)
        {
            var characterId = await _repository.AddCharacterAsync(request.Character);

            var requiredTypes = new[]
            {
                ItemListEnum.Equipment,
                ItemListEnum.Inventory,
                ItemListEnum.Warehouse,
                ItemListEnum.Chipsets,
                ItemListEnum.JogressChipset,
                ItemListEnum.Digivice,
                ItemListEnum.TamerSkill,
                ItemListEnum.RewardWarehouse,
                ItemListEnum.GiftWarehouse,
                ItemListEnum.ConsignedWarehouse,
                ItemListEnum.TamerShop,
                ItemListEnum.ConsignedShop
            };

            foreach (var type in requiredTypes)
                await _sender.Send(new CreateCharacterItemListCommand(characterId, type), cancellationToken);

            return characterId;
        }
    }
}
