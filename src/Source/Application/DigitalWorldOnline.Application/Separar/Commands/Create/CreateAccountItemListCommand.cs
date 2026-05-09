using DigitalWorldOnline.Commons.Enums;
using MediatR;

namespace DigitalWorldOnline.Application.Separar.Commands.Create
{
    /// <summary>
    /// Inserts a single account-level <c>Shared_ItemList</c> row for a legacy account
    /// that's missing one of the four standard account-level lists
    /// (AccountWarehouse, CashWarehouse, ShopWarehouse, BuyHistory). Idempotent — does
    /// nothing if a row of that type already exists for the account.
    /// </summary>
    public class CreateAccountItemListCommand : IRequest
    {
        public long AccountId { get; }
        public ItemListEnum Type { get; }

        public CreateAccountItemListCommand(long accountId, ItemListEnum type)
        {
            AccountId = accountId;
            Type = type;
        }
    }
}
