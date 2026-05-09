using DigitalWorldOnline.Commons.DTOs.Config;
using DigitalWorldOnline.Commons.Enums.ClientEnums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace DigitalWorldOnline.Infraestructure.ContextConfiguration.Config
{
    public class CloneConfiguration : IEntityTypeConfiguration<CloneConfigDTO>
    {
        public void Configure(EntityTypeBuilder<CloneConfigDTO> builder)
        {
            builder
                .ToTable("Config_Clone")
                .HasKey(x => x.Id);

            builder
                .Property(x => x.Type)
                .HasColumnType("int")
                .HasConversion(new ValueConverter<DigicloneTypeEnum, int>(
                    x => (int)x,
                    x => (DigicloneTypeEnum)x))
                .IsRequired();

            builder
                .Property(e => e.Level)
                .HasColumnType("tinyint unsigned")
                .IsRequired();

            builder
                .Property(e => e.SuccessChance)
                 .HasColumnType("decimal(9,2)")
                .HasDefaultValue(0)
                .IsRequired();


            builder
                .Property(e => e.BreakChance)
                .HasColumnType("decimal(9,2)")
                .HasDefaultValue(0)
                .IsRequired();


            builder
                .Property(e => e.MinAmount)
                .HasColumnType("int")
                .IsRequired();

            builder
                .Property(e => e.MaxAmount)
                .HasColumnType("int")
                .IsRequired();
        }
    }
}