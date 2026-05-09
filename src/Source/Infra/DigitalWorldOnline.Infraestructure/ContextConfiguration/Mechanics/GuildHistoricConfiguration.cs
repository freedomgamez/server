using DigitalWorldOnline.Commons.Enums;
using DigitalWorldOnline.Commons.Enums.ClientEnums;
using DigitalWorldOnline.Commons.DTOs.Mechanics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace DigitalWorldOnline.Infraestructure.ContextConfiguration.Mechanics
{
    public class GuildHistoricConfiguration : IEntityTypeConfiguration<GuildHistoricDTO>
    {
        public void Configure(EntityTypeBuilder<GuildHistoricDTO> builder)
        {
            builder
                .ToTable("Guild_Historic")
                .HasKey(x => x.Id);

            builder
                .Property(x => x.Type)
                .HasColumnType("int")
                .HasConversion(new ValueConverter<GuildHistoricTypeEnum, int>(
                    x => (int)x,
                    x => (GuildHistoricTypeEnum)x))
                .IsRequired();

            builder
                .Property(x => x.Date)
                .HasColumnType("datetime(6)")
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)")
                .IsRequired();

            builder
                .Property(x => x.MasterClass)
                .HasColumnType("int")
                .HasConversion(new ValueConverter<GuildAuthorityTypeEnum, int>(
                    x => (int)x,
                    x => (GuildAuthorityTypeEnum)x))
                .IsRequired();

            builder
                .Property(x => x.MasterName)
                .HasColumnType("varchar(255)")
                .HasMaxLength(50)
                .IsRequired();

            builder
                .Property(x => x.MemberClass)
                .HasColumnType("int")
                .HasConversion(new ValueConverter<GuildAuthorityTypeEnum, int>(
                    x => (int)x,
                    x => (GuildAuthorityTypeEnum)x))
                .IsRequired();

            builder
                .Property(x => x.MemberName)
                .HasColumnType("varchar(255)")
                .HasMaxLength(50)
                .IsRequired();
        }
    }
}