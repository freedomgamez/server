using DigitalWorldOnline.Application.GameAssets.Bins;
using DigitalWorldOnline.Commons.DTOs.Assets;
using MediatR;

namespace DigitalWorldOnline.Application.GameAssets.Queries
{
    public class AccessoryRollAssetsQueryHandler : IRequestHandler<AccessoryRollAssetsQuery, List<AccessoryRollAssetDTO>>
    {
        private readonly ItemListBinLoader _itemListBinLoader;

        public AccessoryRollAssetsQueryHandler(ItemListBinLoader itemListBinLoader)
        {
            _itemListBinLoader = itemListBinLoader;
        }

        public Task<List<AccessoryRollAssetDTO>> Handle(AccessoryRollAssetsQuery request, CancellationToken cancellationToken)
        {
            var itemList = _itemListBinLoader.Load();
            var optionByType = itemList.AccessoryOptions
                .GroupBy(x => x.ItemType)
                .ToDictionary(x => x.Key, x => x.First());

            var records = new List<AccessoryRollAssetDTO>(optionByType.Count);
            long rollId = 1;
            long statusId = 1;

            foreach (var (itemType, optionRecord) in optionByType)
            {
                var enchantRecord = itemList.AccessoryEnchants.FirstOrDefault(x => x.ItemType == itemType);
                int rerollAmount = enchantRecord != null ? Math.Max(0, (int)enchantRecord.MaxValue) : 0;

                var status = optionRecord.Options
                    .Where(x => x.OptionType > 0)
                    .Select(x => new AccessoryRollStatusAssetDTO
                    {
                        Id = statusId++,
                        Type = x.OptionType,
                        MinValue = (int)Math.Clamp((long)x.MinValue, int.MinValue, int.MaxValue),
                        MaxValue = (int)Math.Clamp((long)x.MaxValue, int.MinValue, int.MaxValue),
                        AccessoryRollAssetId = rollId
                    })
                    .ToList();

                records.Add(new AccessoryRollAssetDTO
                {
                    Id = rollId,
                    ItemId = (int)itemType,
                    StatusAmount = Math.Max(0, (int)optionRecord.MaxValue),
                    RerollAmount = rerollAmount,
                    Status = status
                });

                rollId++;
            }

            return Task.FromResult(records);
        }
    }
}
