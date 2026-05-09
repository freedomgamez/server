using DigitalWorldOnline.Commons.DTOs.Config;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DigitalWorldOnline.Infraestructure.ContextConfiguration.Config
{
    public class SummonMobLocationConfiguration : IEntityTypeConfiguration<SummonMobLocationDTO>
    {
        public void Configure(EntityTypeBuilder<SummonMobLocationDTO> builder)
        {
            builder
                .ToTable("Config_SummonMobLocation")
                .HasKey(x => x.Id);

            builder
                .Property(x => x.MapId)
                .HasColumnType("smallint")
                .HasDefaultValue(0)
                .IsRequired();

            builder
                .Property(x => x.X)
                .HasColumnType("int")
                .HasDefaultValue(5000)
                .IsRequired();

            builder
                .Property(x => x.Y)
                .HasColumnType("int")
                .HasDefaultValue(4500)
                .IsRequired();

            builder
                .Property(x => x.Z)
                .HasColumnType("decimal(9,2)")
                .HasDefaultValue(0)
                .IsRequired();
            ;
        }
    }
}