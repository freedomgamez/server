using DigitalWorldOnline.Commons.Enums;
using DigitalWorldOnline.Commons.DTOs.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace DigitalWorldOnline.Infraestructure.ContextConfiguration.Event
{
    public class TimeRewardConfiguration : IEntityTypeConfiguration<TimeRewardDTO>
    {
        public void Configure(EntityTypeBuilder<TimeRewardDTO> builder)
        {
            builder
                .ToTable("Event_TimeReward")
                .HasKey(x => x.Id);

            builder
                .Property(x => x.RewardIndex)
                .HasColumnType("int")
                .HasConversion(new ValueConverter<TimeRewardIndexEnum, int>(
                    x => (int)x,
                    x => (TimeRewardIndexEnum)x))
                .HasDefaultValue(TimeRewardIndexEnum.First)
                .IsRequired();

            builder
                .Property(x => x.StartTime)
                .HasColumnType("datetime(6)")
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)")
                .IsRequired();

            // RemainingSeconds: in-session-decremented countdown to the current threshold.
            // Default 1800 = 30 min (matches First-tier duration); existing rows from prior
            // builds get reset to First-tier on migration. Frozen across logouts so offline
            // time doesn't burn the timer.
            builder
                .Property(x => x.RemainingSeconds)
                .HasColumnType("int")
                .HasDefaultValue(1800)
                .IsRequired();
        }
    }
}