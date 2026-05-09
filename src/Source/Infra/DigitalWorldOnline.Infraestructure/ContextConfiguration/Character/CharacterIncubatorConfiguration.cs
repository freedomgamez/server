using DigitalWorldOnline.Commons.DTOs.Character;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DigitalWorldOnline.Infraestructure.ContextConfiguration.Character
{
    public class CharacterIncubatorConfiguration : IEntityTypeConfiguration<CharacterIncubatorDTO>
    {
        public void Configure(EntityTypeBuilder<CharacterIncubatorDTO> builder)
        {
            builder
                .ToTable("Character_Incubator")
                .HasKey(x => x.Id);

            builder
                .Property(x => x.EggId)
                .HasColumnType("int")
                .HasDefaultValue(byte.MinValue)
                .IsRequired();
            
            builder
                .Property(x => x.HatchLevel)
                .HasColumnType("tinyint unsigned")
                .HasDefaultValue(byte.MinValue)
                .IsRequired();
            
            builder
                .Property(x => x.BackupDiskId)
                .HasColumnType("int")
                .HasDefaultValue(byte.MinValue)
                .IsRequired();
        }
    }
}