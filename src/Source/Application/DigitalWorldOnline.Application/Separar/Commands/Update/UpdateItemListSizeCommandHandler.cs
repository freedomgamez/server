using DigitalWorldOnline.Application.Services;
using MediatR;

namespace DigitalWorldOnline.Application.Separar.Commands.Update
{
    public class UpdateItemListSizeCommandHandler : IRequestHandler<UpdateItemListSizeCommand>
    {
        private readonly StoragePersistenceDualWriteCoordinator _dualWriteCoordinator;

        public UpdateItemListSizeCommandHandler(StoragePersistenceDualWriteCoordinator dualWriteCoordinator)
        {
            _dualWriteCoordinator = dualWriteCoordinator;
        }

        public async Task<Unit> Handle(UpdateItemListSizeCommand request, CancellationToken cancellationToken)
        {
            await _dualWriteCoordinator.UpdateItemListSizeAsync(request.ItemListId, request.NewSize);

            return Unit.Value;
        }
    }
}
