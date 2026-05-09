using DigitalWorldOnline.Commons.Enums.ClientEnums;
using DigitalWorldOnline.Commons.DTOs.Server;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using DigitalWorldOnline.Commons.Enums.Server;

namespace DigitalWorldOnline.Infraestructure.ContextConfiguration.Config
{
    public class ServerConfigConfiguration : IEntityTypeConfiguration<ServerDTO>
    {
        public void Configure(EntityTypeBuilder<ServerDTO> builder)
        {
            builder
                .ToTable("Account_Server")
                .HasKey(x => x.Id);

            builder
                .Property(x => x.Name)
                .HasColumnType("varchar(255)")
                .HasMaxLength(20)
                .IsRequired();

            builder
                .Property(x => x.Experience)
                .HasColumnType("int")
                .HasDefaultValue(0)
                .IsRequired();

            builder
                .Property(x => x.Maintenance)
                .HasColumnType("tinyint(1)")
                .HasDefaultValue(false)
                .IsRequired();

            builder
                .Property(x => x.New)
                .HasColumnType("tinyint(1)")
                .IsRequired();
            
            builder
                .Property(x => x.Port)
                .HasColumnType("int")
                .HasDefaultValue(0)
                .IsRequired();

            builder
                .Property(x => x.Overload)
                .HasColumnType("int")
                .HasConversion(new ValueConverter<ServerOverloadEnum, int>(
                    x => (int)x,
                    x => (ServerOverloadEnum)x))
                .HasDefaultValue(ServerOverloadEnum.Empty)
                .IsRequired();
            
            builder
                .Property(x => x.Type)
                .HasColumnType("int")
                .HasConversion(new ValueConverter<ServerTypeEnum, int>(
                    x => (int)x,
                    x => (ServerTypeEnum)x))
                .HasDefaultValue(ServerTypeEnum.Default)
                .IsRequired();

            builder
                .Property(x => x.CreateDate)
                .HasColumnType("datetime(6)")
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)")
                .IsRequired();
        }
    }
}