using DigitalWorldOnline.Application.Services;
using MediatR;

namespace DigitalWorldOnline.Application.Separar.Commands.Update
{
    public class UpdateItemListBitsCommandHandler : IRequestHandler<UpdateItemListBitsCommand>
    {
        private readonly StoragePersistenceDualWriteCoordinator _dualWriteCoordinator;

        public UpdateItemListBitsCommandHandler(StoragePersistenceDualWriteCoordinator dualWriteCoordinator)
        {
            _dualWriteCoordinator = dualWriteCoordinator;
        }

        public async Task<Unit> Handle(UpdateItemListBitsCommand request, CancellationToken cancellationToken)
        {
            await _dualWriteCoordinator.UpdateItemListBitsAsync(request.ItemListId, request.Bits);

            return Unit.Value;
        }
    }
}
