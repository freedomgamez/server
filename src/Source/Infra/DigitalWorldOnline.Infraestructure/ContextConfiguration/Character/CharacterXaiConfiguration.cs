using DigitalWorldOnline.Commons.DTOs.Character;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DigitalWorldOnline.Infraestructure.ContextConfiguration.Character
{
    public class CharacterXaiConfiguration : IEntityTypeConfiguration<CharacterXaiDTO>
    {
        public void Configure(EntityTypeBuilder<CharacterXaiDTO> builder)
        {
            builder
                .ToTable("Character_XaiInfo")
                .HasKey(x => x.Id);

            builder
                .Property(x => x.ItemId)
                .HasColumnType("int")
                .HasDefaultValue(0)
                .IsRequired();
            
            builder
                .Property(x => x.XGauge)
                .HasColumnType("smallint")
                .HasDefaultValue(0)
                .IsRequired();
            
            builder
                .Property(x => x.XCrystals)
                .HasColumnType("tinyint unsigned")
                .HasDefaultValue(byte.MinValue)
                .IsRequired();
        }
    }
}