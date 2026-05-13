using DigitalWorldOnline.Application.Separar.Commands.Create;
using DigitalWorldOnline.Commons.DTOs.Account;
using DigitalWorldOnline.Commons.Enums;
using DigitalWorldOnline.Commons.Repositories.Admin;
using MediatR;

namespace DigitalWorldOnline.Application.Admin.Commands
{
    public class CreateAccountCommandHandler : IRequestHandler<CreateAccountCommand, AccountDTO>
    {
        private readonly IAdminCommandsRepository _repository;
        private readonly ISender _sender;

        public CreateAccountCommandHandler(IAdminCommandsRepository repository, ISender sender)
        {
            _repository = repository;
            _sender = sender;
        }

        public async Task<AccountDTO> Handle(CreateAccountCommand request, CancellationToken cancellationToken)
        {
            var account = await _repository.AddAccountAsync(request.Account);

            var requiredTypes = new[]
            {
                ItemListEnum.AccountWarehouse,
                ItemListEnum.CashWarehouse,
                ItemListEnum.ShopWarehouse,
                ItemListEnum.BuyHistory
            };

            foreach (var type in requiredTypes)
                await _sender.Send(new CreateAccountItemListCommand(account.Id, type), cancellationToken);

            return account;
        }
    }
}
