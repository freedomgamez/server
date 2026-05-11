using DigitalWorldOnline.Commons.Interfaces;
using MediatR;

namespace DigitalWorldOnline.Application.Separar.Commands.Update
{
    public sealed class UpdateMemorySkillCooldownCommandHandler : IRequestHandler<UpdateMemorySkillCooldownCommand>
    {
        private readonly ICharacterCommandsRepository _repository;

        public UpdateMemorySkillCooldownCommandHandler(ICharacterCommandsRepository repository)
        {
            _repository = repository;
        }

        public async Task<Unit> Handle(UpdateMemorySkillCooldownCommand request, CancellationToken cancellationToken)
        {
            await _repository.UpdateMemorySkillCooldownAsync(request.EvolutionId, request.SkillId, request.CooldownEndsAt);
            return Unit.Value;
        }
    }
}
