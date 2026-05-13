using DigitalWorldOnline.Infraestructure.Repositories.Shared.ReadModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DigitalWorldOnline.Infraestructure.ContextConfiguration.Shared
{
    public class NormalizedItemSlotConfiguration : IEntityTypeConfiguration<NormalizedItemSlotReadModel>
    {
        public void Configure(EntityTypeBuilder<NormalizedItemSlotReadModel> builder)
        {
            builder
                .ToTable("Shared_ItemSlotNormalized")
                .HasKey(x => x.Id);

            builder
                .Property(x => x.ItemListId)
                .HasColumnType("bigint")
                .IsRequired();

            builder
                .Property(x => x.Slot)
                .HasColumnType("smallint unsigned")
                .IsRequired();

            builder
                .Property(x => x.ItemInstanceId)
                .HasColumnType("char(36)");

            builder
                .Property(x => x.UpdatedAt)
                .HasColumnType("datetime(6)")
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)")
                .IsRequired();

            builder
                .HasIndex(x => new { x.ItemListId, x.Slot })
                .IsUnique()
                .HasDatabaseName("UX_Shared_ItemSlotNormalized_ItemList_Slot");

            builder
                .HasIndex(x => x.ItemInstanceId)
                .HasDatabaseName("IX_Shared_ItemSlotNormalized_ItemInstanceId");

            builder
                .HasOne<NormalizedItemListReadModel>()
                .WithMany()
                .HasForeignKey(x => x.ItemListId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Shared_ItemSlotNormalized_Shared_ItemListNormalized_ItemListId");

            builder
                .HasOne<NormalizedItemInstanceReadModel>()
                .WithMany()
                .HasForeignKey(x => x.ItemInstanceId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_Shared_ItemSlotNormalized_Shared_ItemInstanceNormalized_ItemInstanceId");
        }
    }
}
