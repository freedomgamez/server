namespace DigitalWorldOnline.Infraestructure.Repositories.Shared.ReadModels
{
    public sealed class OwnerItemStorageAccountSlotReadModel
    {
        public long AccountId { get; set; }
        public int Type { get; set; }
        public short Slot { get; set; }
        public Guid? ItemInstanceId { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
