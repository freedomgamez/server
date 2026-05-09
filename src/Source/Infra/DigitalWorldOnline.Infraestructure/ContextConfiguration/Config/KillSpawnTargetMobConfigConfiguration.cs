using DigitalWorldOnline.Commons.DTOs.Config;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DigitalWorldOnline.Infraestructure.ContextConfiguration.Config
{
    public class KillSpawnTargetMobConfigConfiguration : IEntityTypeConfiguration<KillSpawnTargetMobConfigDTO>
    {
        public void Configure(EntityTypeBuilder<KillSpawnTargetMobConfigDTO> builder)
        {
            builder
                .ToTable("Config_KillSpawnTargetMob")
                .HasKey(x => x.Id);

            builder
                .Property(e => e.TargetMobType)
                .HasColumnType("int")
                .IsRequired();

            builder
                .Property(e => e.TargetMobAmount)
                .HasColumnType("tinyint unsigned")
                .IsRequired();

        }
    }
}