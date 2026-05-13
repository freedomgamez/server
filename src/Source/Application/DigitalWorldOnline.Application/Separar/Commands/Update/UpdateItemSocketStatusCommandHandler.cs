using DigitalWorldOnline.Application.Services;
using MediatR;

namespace DigitalWorldOnline.Application.Separar.Commands.Update
{
    public class UpdateItemSocketStatusCommandHandler : IRequestHandler<UpdateItemSocketStatusCommand>
    {
        private readonly StoragePersistenceDualWriteCoordinator _dualWriteCoordinator;

        public UpdateItemSocketStatusCommandHandler(StoragePersistenceDualWriteCoordinator dualWriteCoordinator)
        {
            _dualWriteCoordinator = dualWriteCoordinator;
        }

        public async Task<Unit> Handle(UpdateItemSocketStatusCommand request, CancellationToken cancellationToken)
        {
            await _dualWriteCoordinator.UpdateItemSocketStatusAsync(request.Item);

            return Unit.Value;
        }
    }
}
