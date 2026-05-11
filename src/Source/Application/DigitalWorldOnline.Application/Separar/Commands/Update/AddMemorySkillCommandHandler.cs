using DigitalWorldOnline.Commons.Interfaces;
using MediatR;

namespace DigitalWorldOnline.Application.Separar.Commands.Update
{
    public sealed class AddMemorySkillCommandHandler : IRequestHandler<AddMemorySkillCommand, long>
    {
        private readonly ICharacterCommandsRepository _repository;

        public AddMemorySkillCommandHandler(ICharacterCommandsRepository repository)
        {
            _repository = repository;
        }

        public Task<long> Handle(AddMemorySkillCommand request, CancellationToken cancellationToken)
            => _repository.AddMemorySkillAsync(request.EvolutionId, request.SkillId, request.MaxLevel);
    }
}
