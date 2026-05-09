using DigitalWorldOnline.Commons.DTOs.Config;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DigitalWorldOnline.Infraestructure.ContextConfiguration.Config
{
    public class MobDropRewardConfigConfiguration : IEntityTypeConfiguration<MobDropRewardConfigDTO>
    {
        public void Configure(EntityTypeBuilder<MobDropRewardConfigDTO> builder)
        {
            builder
                .ToTable("Config_MobDropReward")
                .HasKey(x => x.Id);

            builder
                .Property(e => e.MinAmount)
                .HasColumnType("tinyint unsigned")
                .HasDefaultValue(byte.MinValue)
                .IsRequired();

            builder
                .Property(e => e.MaxAmount)
                .HasColumnType("tinyint unsigned")
                .HasDefaultValue(byte.MinValue)
                .IsRequired();

            builder
                .HasMany(x => x.Drops)
                .WithOne(x => x.DropReward)
                .HasForeignKey(x => x.DropRewardId);

            builder
                .HasOne(x => x.BitsDrop)
                .WithOne(x => x.DropReward)
                .HasForeignKey<BitsDropConfigDTO>(x => x.DropRewardId);
        }
    }
}