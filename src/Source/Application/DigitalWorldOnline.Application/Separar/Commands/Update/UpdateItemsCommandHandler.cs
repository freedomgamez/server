using DigitalWorldOnline.Application.Services;
using MediatR;

namespace DigitalWorldOnline.Application.Separar.Commands.Update
{
    public class UpdateItemsCommandHandler : IRequestHandler<UpdateItemsCommand>
    {
        private readonly StoragePersistenceDualWriteCoordinator _dualWriteCoordinator;

        public UpdateItemsCommandHandler(StoragePersistenceDualWriteCoordinator dualWriteCoordinator)
        {
            _dualWriteCoordinator = dualWriteCoordinator;
        }

        public async Task<Unit> Handle(UpdateItemsCommand request, CancellationToken cancellationToken)
        {
            await _dualWriteCoordinator.UpdateItemsAsync(request.Items);

            return Unit.Value;
        }
    }
}
