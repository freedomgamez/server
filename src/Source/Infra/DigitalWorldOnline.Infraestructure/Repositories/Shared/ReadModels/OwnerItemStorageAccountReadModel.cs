namespace DigitalWorldOnline.Infraestructure.Repositories.Shared.ReadModels
{
    public sealed class OwnerItemStorageAccountReadModel
    {
        public long AccountId { get; set; }
        public int Type { get; set; }
        public short Size { get; set; }
        public long Bits { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
