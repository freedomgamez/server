using System.ComponentModel.DataAnnotations;

namespace DigitalWorldOnline.Infraestructure.Repositories.Shared.ReadModels
{
    public sealed class NormalizedItemInstanceSocketStatusReadModel
    {
        [Key]
        public Guid Id { get; set; }
        public Guid ItemInstanceId { get; set; }
        public byte Slot { get; set; }
        public short Type { get; set; }
        public short AttributeId { get; set; }
        public short Value { get; set; }
    }
}
