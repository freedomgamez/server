using DigitalWorldOnline.Commons.DTOs.Character;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DigitalWorldOnline.Infraestructure.ContextConfiguration.Character
{
    public class CharacterMapRegionConfiguration : IEntityTypeConfiguration<CharacterMapRegionDTO>
    {
        public void Configure(EntityTypeBuilder<CharacterMapRegionDTO> builder)
        {
            builder
                .ToTable("Character_MapRegion")
                .HasKey(x => x.Id);

            builder
                .Property(x => x.Unlocked)
                .HasColumnType("tinyint unsigned")
                .HasDefaultValue(0)
                .IsRequired();
        }
    }
}