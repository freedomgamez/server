using System.ComponentModel.DataAnnotations;

namespace DigitalWorldOnline.Infraestructure.Repositories.Shared.ReadModels
{
    public sealed class OwnerItemStorageInstanceReadModel
    {
        [Key]
        public Guid Id { get; set; }
        public int ItemId { get; set; }
        public int Amount { get; set; }
        public byte Power { get; set; }
        public byte RerollLeft { get; set; }
        public byte FamilyType { get; set; }
        public int Duration { get; set; }
        public DateTime EndDate { get; set; }
        public bool FirstExpired { get; set; }
        public int TamerShopSellPrice { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
