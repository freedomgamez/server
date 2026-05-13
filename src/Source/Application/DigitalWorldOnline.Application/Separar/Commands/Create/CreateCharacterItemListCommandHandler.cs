using DigitalWorldOnline.Commons.Interfaces;
using MediatR;

namespace DigitalWorldOnline.Application.Separar.Commands.Create
{
    public class CreateCharacterItemListCommandHandler : IRequestHandler<CreateCharacterItemListCommand>
    {
        private readonly ICharacterCommandsRepository _repository;

        public CreateCharacterItemListCommandHandler(ICharacterCommandsRepository repository)
        {
            _repository = repository;
        }

        public async Task<Unit> Handle(CreateCharacterItemListCommand request, CancellationToken cancellationToken)
        {
            await _repository.EnsureCharacterItemListAsync(request.CharacterId, request.Type);
            return Unit.Value;
        }
    }
}
