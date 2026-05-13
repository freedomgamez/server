using DigitalWorldOnline.Application.GameAssets.Bins;
using DigitalWorldOnline.Commons.DTOs.Assets;
using MediatR;

namespace DigitalWorldOnline.Application.GameAssets.Queries
{
    public class ItemAssetsQueryHandler : IRequestHandler<ItemAssetsQuery, List<ItemAssetDTO>>
    {
        private readonly ItemListBinLoader _itemListBinLoader;

        public ItemAssetsQueryHandler(ItemListBinLoader itemListBinLoader)
        {
            _itemListBinLoader = itemListBinLoader;
        }

        public async Task<List<ItemAssetDTO>> Handle(ItemAssetsQuery request, CancellationToken cancellationToken)
        {
            var data = _itemListBinLoader.Load();
            long id = 1;
            foreach (var item in data.Items)
            {
                item.Id = id++;
            }
            return await Task.FromResult(data.Items.ToList());
        }
    }
}
