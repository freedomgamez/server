using DigitalWorldOnline.Commons.Interfaces;
using DigitalWorldOnline.Commons.Models.Base;

namespace DigitalWorldOnline.Application.Services
{
    public sealed class StoragePersistenceDualWriteCoordinator
    {
        private readonly ICharacterCommandsRepository _repository;

        public StoragePersistenceDualWriteCoordinator(ICharacterCommandsRepository repository)
        {
            _repository = repository;
        }

        public Task UpdateItemsAsync(List<ItemModel> items)
            => _repository.UpdateItemsAsync(items);

        public Task UpdateItemListBitsAsync(long itemListId, long bits)
            => _repository.UpdateItemListBitsAsync(itemListId, bits);

        public Task UpdateItemListSizeAsync(long itemListId, byte newSize)
            => _repository.UpdateItemListSizeAsync(itemListId, newSize);

        public Task UpdateItemAsync(ItemModel item)
            => _repository.UpdateItemAsync(item);

        public Task UpdateItemAccessoryStatusAsync(ItemModel item)
            => _repository.UpdateItemAccessoryStatusAsync(item);

        public Task UpdateItemSocketStatusAsync(ItemModel item)
            => _repository.UpdateItemSocketStatusAsync(item);
    }
}
