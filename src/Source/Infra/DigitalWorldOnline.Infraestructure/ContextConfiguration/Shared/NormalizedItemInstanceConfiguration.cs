using DigitalWorldOnline.Infraestructure.Repositories.Shared.ReadModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DigitalWorldOnline.Infraestructure.ContextConfiguration.Shared
{
    public class NormalizedItemInstanceConfiguration : IEntityTypeConfiguration<NormalizedItemInstanceReadModel>
    {
        public void Configure(EntityTypeBuilder<NormalizedItemInstanceReadModel> builder)
        {
            builder
                .ToTable("Shared_ItemInstanceNormalized")
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
                .Property(x => x.FamilyType)
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
                .Property(x => x.TamerShopSellPrice)
                .HasColumnType("int")
                .HasDefaultValue(0)
                .IsRequired();

            builder
                .Property(x => x.CreatedAt)
                .HasColumnType("datetime(6)")
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)")
                .IsRequired();

            builder
                .HasIndex(x => x.ItemId)
                .HasDatabaseName("IX_Shared_ItemInstanceNormalized_ItemId");
        }
    }
}
