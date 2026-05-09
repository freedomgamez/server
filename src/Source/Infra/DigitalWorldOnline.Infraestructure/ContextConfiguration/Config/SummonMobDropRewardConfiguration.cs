using DigitalWorldOnline.Commons.DTOs.Config;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalWorldOnline.Infraestructure.ContextConfiguration.Config
{
    public class SummonMobDropRewardConfiguration : IEntityTypeConfiguration<SummonMobDropRewardDTO>
    {
        public void Configure(EntityTypeBuilder<SummonMobDropRewardDTO> builder)
        {
            builder
                .ToTable("Config_SummonMobDropReward")
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
                .HasForeignKey<SummonMobBitDropDTO>(x => x.DropRewardId);
        }
    }
}
