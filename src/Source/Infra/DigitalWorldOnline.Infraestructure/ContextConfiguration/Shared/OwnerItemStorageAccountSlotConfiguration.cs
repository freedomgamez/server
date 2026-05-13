using DigitalWorldOnline.Infraestructure.Repositories.Shared.ReadModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DigitalWorldOnline.Infraestructure.ContextConfiguration.Shared
{
    public sealed class OwnerItemStorageAccountSlotConfiguration : IEntityTypeConfiguration<OwnerItemStorageAccountSlotReadModel>
    {
        public void Configure(EntityTypeBuilder<OwnerItemStorageAccountSlotReadModel> builder)
        {
            builder
                .ToTable("Shared_ItemStorageAccountSlot")
                .HasKey(x => new { x.AccountId, x.Type, x.Slot });

            builder.Property(x => x.AccountId).HasColumnType("bigint").IsRequired();
            builder.Property(x => x.Type).HasColumnType("int").IsRequired();
            builder.Property(x => x.Slot).HasColumnType("smallint unsigned").IsRequired();
            builder.Property(x => x.ItemInstanceId).HasColumnType("char(36)");
            builder.Property(x => x.UpdatedAt).HasColumnType("datetime(6)").HasDefaultValueSql("CURRENT_TIMESTAMP(6)").IsRequired();

            builder
                .HasIndex(x => x.ItemInstanceId)
                .HasDatabaseName("IX_Shared_ItemStorageAccountSlot_ItemInstanceId");

            builder
                .HasOne<OwnerItemStorageAccountReadModel>()
                .WithMany()
                .HasForeignKey(x => new { x.AccountId, x.Type })
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Shared_ItemStorageAccountSlot_Shared_ItemStorageAccount");

            builder
                .HasOne<OwnerItemStorageInstanceReadModel>()
                .WithMany()
                .HasForeignKey(x => x.ItemInstanceId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_Shared_ItemStorageAccountSlot_Shared_ItemStorageInstance");
        }
    }
}
