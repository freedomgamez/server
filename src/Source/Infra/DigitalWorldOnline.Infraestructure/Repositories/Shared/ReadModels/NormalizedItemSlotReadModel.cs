using System.ComponentModel.DataAnnotations;

namespace DigitalWorldOnline.Infraestructure.Repositories.Shared.ReadModels
{
    public sealed class NormalizedItemSlotReadModel
    {
        [Key]
        public long Id { get; set; }
        public long ItemListId { get; set; }
        public short Slot { get; set; }
        public Guid? ItemInstanceId { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
