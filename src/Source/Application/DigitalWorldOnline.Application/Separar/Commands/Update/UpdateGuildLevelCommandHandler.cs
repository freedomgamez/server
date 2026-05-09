using DigitalWorldOnline.Commons.Interfaces;
using MediatR;

namespace DigitalWorldOnline.Application.Separar.Commands.Update
{
    public class UpdateGuildLevelCommandHandler : IRequestHandler<UpdateGuildLevelCommand>
    {
        private readonly IServerCommandsRepository _repository;

        public UpdateGuildLevelCommandHandler(IServerCommandsRepository repository)
        {
            _repository = repository;
        }

        public async Task<Unit> Handle(UpdateGuildLevelCommand request, CancellationToken cancellationToken)
        {
            await _repository.UpdateGuildLevelAsync(request.GuildId, request.Level, request.CurrentExperience);
            return Unit.Value;
        }
    }
}
