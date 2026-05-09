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
        }
    }
}