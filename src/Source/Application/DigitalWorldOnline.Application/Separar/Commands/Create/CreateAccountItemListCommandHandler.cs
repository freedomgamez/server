using DigitalWorldOnline.Commons.Interfaces;
using MediatR;

namespace DigitalWorldOnline.Application.Separar.Commands.Create
{
    public class CreateAccountItemListCommandHandler : IRequestHandler<CreateAccountItemListCommand>
    {
        private readonly IAccountCommandsRepository _repository;

        public CreateAccountItemListCommandHandler(IAccountCommandsRepository repository)
        {
            _repository = repository;
        }

        public async Task<Unit> Handle(CreateAccountItemListCommand request, CancellationToken cancellationToken)
        {
            await _repository.EnsureAccountItemListAsync(request.AccountId, request.Type);
            return Unit.Value;
        }
    }
}
