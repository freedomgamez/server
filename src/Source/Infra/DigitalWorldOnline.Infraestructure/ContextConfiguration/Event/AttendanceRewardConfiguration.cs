using DigitalWorldOnline.Commons.DTOs.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DigitalWorldOnline.Infraestructure.ContextConfiguration.Event
{
    public class AttendanceRewardConfiguration : IEntityTypeConfiguration<AttendanceRewardDTO>
    {
        public void Configure(EntityTypeBuilder<AttendanceRewardDTO> builder)
        {
            builder
                .ToTable("Event_AttendanceReward")
                .HasKey(x => x.Id);

            builder
                .Property(x => x.TotalDays)
                .HasColumnType("tinyint unsigned")
                .HasDefaultValue(byte.MinValue)
                .IsRequired();
        }
    }
}