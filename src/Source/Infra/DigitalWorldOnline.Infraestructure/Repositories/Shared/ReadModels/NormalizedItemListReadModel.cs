using System.ComponentModel.DataAnnotations;

namespace DigitalWorldOnline.Infraestructure.Repositories.Shared.ReadModels
{
    public sealed class NormalizedItemListReadModel
    {
        [Key]
        public long Id { get; set; }
        public long? AccountId { get; set; }
        public long? CharacterId { get; set; }
        public int Type { get; set; }
        public short Size { get; set; }
        public long Bits { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
