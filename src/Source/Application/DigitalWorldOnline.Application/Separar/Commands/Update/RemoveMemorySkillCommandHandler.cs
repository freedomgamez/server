using DigitalWorldOnline.Commons.Interfaces;
using MediatR;

namespace DigitalWorldOnline.Application.Separar.Commands.Update
{
    public sealed class RemoveMemorySkillCommandHandler : IRequestHandler<RemoveMemorySkillCommand, bool>
    {
        private readonly ICharacterCommandsRepository _repository;

        public RemoveMemorySkillCommandHandler(ICharacterCommandsRepository repository)
        {
            _repository = repository;
        }

        public Task<bool> Handle(RemoveMemorySkillCommand request, CancellationToken cancellationToken)
            => _repository.RemoveMemorySkillAsync(request.EvolutionId, request.SkillId);
    }
}
