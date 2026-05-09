using DigitalWorldOnline.Commons.DTOs.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DigitalWorldOnline.Infraestructure.ContextConfiguration.Shared
{
    public class ItemConfiguration : IEntityTypeConfiguration<ItemDTO>
    {
        public void Configure(EntityTypeBuilder<ItemDTO> builder)
        {
            builder
                .ToTable("Shared_Item")
                .HasKey(x => x.Id);

            builder
                .Property(x => x.ItemId)
                .HasColumnType("int")
                .IsRequired();

            builder
                .Property(x => x.Amount)
                .HasColumnType("int")
                .IsRequired();

            builder
                .Property(x => x.Power)
                .HasColumnType("tinyint unsigned")
                .IsRequired();

            builder
                .Property(x => x.RerollLeft)
                .HasColumnType("tinyint unsigned")
                .IsRequired();

            builder
               .Property(x => x.Duration)
               .HasColumnType("int")
               .IsRequired();

            builder
               .Property(x => x.EndDate)
               .HasColumnType("datetime(6)")
               .HasDefaultValueSql("CURRENT_TIMESTAMP(6)")
               .IsRequired();

            builder
                .Property(x => x.FirstExpired)
                .HasColumnType("tinyint(1)")
                .HasDefaultValue(1)
                .IsRequired();

            builder
            .Property(x => x.FamilyType)
            .HasColumnType("tinyint unsigned")
            .IsRequired();

            builder
                .Property(x => x.Slot)
                .HasColumnType("int")
                .IsRequired();

            builder
                .HasMany(x => x.AccessoryStatus)
                .WithOne(x => x.Item)
                .HasForeignKey(x => x.ItemId);
        }
    }
}