using DigitalWorldOnline.Application.Services;
using MediatR;

namespace DigitalWorldOnline.Application.Separar.Commands.Update
{
    public class UpdateItemAccessoryStatusCommandHandler : IRequestHandler<UpdateItemAccessoryStatusCommand>
    {
        private readonly StoragePersistenceDualWriteCoordinator _dualWriteCoordinator;

        public UpdateItemAccessoryStatusCommandHandler(StoragePersistenceDualWriteCoordinator dualWriteCoordinator)
        {
            _dualWriteCoordinator = dualWriteCoordinator;
        }

        public async Task<Unit> Handle(UpdateItemAccessoryStatusCommand request, CancellationToken cancellationToken)
        {
            await _dualWriteCoordinator.UpdateItemAccessoryStatusAsync(request.Item);

            return Unit.Value;
        }
    }
}
