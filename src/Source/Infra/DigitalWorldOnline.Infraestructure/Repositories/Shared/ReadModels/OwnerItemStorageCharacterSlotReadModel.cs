namespace DigitalWorldOnline.Infraestructure.Repositories.Shared.ReadModels
{
    public sealed class OwnerItemStorageCharacterSlotReadModel
    {
        public long CharacterId { get; set; }
        public int Type { get; set; }
        public short Slot { get; set; }
        public Guid? ItemInstanceId { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
