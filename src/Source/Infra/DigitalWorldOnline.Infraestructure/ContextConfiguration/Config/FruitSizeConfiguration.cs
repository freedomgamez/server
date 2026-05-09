using DigitalWorldOnline.Commons.DTOs.Config;
using DigitalWorldOnline.Commons.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace DigitalWorldOnline.Infraestructure.ContextConfiguration.Config
{
    public class FruitSizeConfiguration : IEntityTypeConfiguration<FruitSizeConfigDTO>
    {
        public void Configure(EntityTypeBuilder<FruitSizeConfigDTO> builder)
        {
            builder
                .ToTable("Config_FruitSize")
                .HasKey(x => x.Id);

            builder
                .Property(x => x.HatchGrade)
                .HasColumnType("int")
                .HasConversion(new ValueConverter<DigimonHatchGradeEnum, int>(
                    x => (int)x,
                    x => (DigimonHatchGradeEnum)x))
                .IsRequired();

            builder
                .Property(x => x.Size)
                 .HasColumnType("decimal(9,2)")
                .HasDefaultValue(0)
                .IsRequired();


            builder
                .Property(x => x.Chance)
                 .HasColumnType("decimal(9,2)")
                .HasDefaultValue(0)
                .IsRequired();

        }
    }
}