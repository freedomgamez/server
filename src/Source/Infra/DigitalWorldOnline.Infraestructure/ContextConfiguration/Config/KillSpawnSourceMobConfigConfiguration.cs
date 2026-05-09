using DigitalWorldOnline.Commons.DTOs.Config;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DigitalWorldOnline.Infraestructure.ContextConfiguration.Config
{
    public class KillSpawnSourceMobConfigConfiguration : IEntityTypeConfiguration<KillSpawnSourceMobConfigDTO>
    {
        public void Configure(EntityTypeBuilder<KillSpawnSourceMobConfigDTO> builder)
        {
            builder
                .ToTable("Config_KillSpawnSourceMob")
                .HasKey(x => x.Id);

            builder
                .Property(e => e.SourceMobType)
                .HasColumnType("int")
                .IsRequired();

            builder
                .Property(e => e.SourceMobRequiredAmount)
                .HasColumnType("tinyint unsigned")
                .IsRequired();
        }
    }
}