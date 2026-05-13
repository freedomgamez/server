using DigitalWorldOnline.Application.Services;
using MediatR;

namespace DigitalWorldOnline.Application.Separar.Commands.Update
{
    public class UpdateItemCommandHandler : IRequestHandler<UpdateItemCommand>
    {
        private readonly StoragePersistenceDualWriteCoordinator _dualWriteCoordinator;

        public UpdateItemCommandHandler(StoragePersistenceDualWriteCoordinator dualWriteCoordinator)
        {
            _dualWriteCoordinator = dualWriteCoordinator;
        }

        public async Task<Unit> Handle(UpdateItemCommand request, CancellationToken cancellationToken)
        {
            await _dualWriteCoordinator.UpdateItemAsync(request.Item);

            return Unit.Value;
        }
    }
}
