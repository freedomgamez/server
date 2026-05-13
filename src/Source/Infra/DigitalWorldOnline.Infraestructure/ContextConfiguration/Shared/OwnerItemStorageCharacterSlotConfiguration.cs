using DigitalWorldOnline.Infraestructure.Repositories.Shared.ReadModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DigitalWorldOnline.Infraestructure.ContextConfiguration.Shared
{
    public sealed class OwnerItemStorageCharacterSlotConfiguration : IEntityTypeConfiguration<OwnerItemStorageCharacterSlotReadModel>
    {
        public void Configure(EntityTypeBuilder<OwnerItemStorageCharacterSlotReadModel> builder)
        {
            builder
                .ToTable("Shared_ItemStorageCharacterSlot")
                .HasKey(x => new { x.CharacterId, x.Type, x.Slot });

            builder.Property(x => x.CharacterId).HasColumnType("bigint").IsRequired();
            builder.Property(x => x.Type).HasColumnType("int").IsRequired();
            builder.Property(x => x.Slot).HasColumnType("smallint unsigned").IsRequired();
            builder.Property(x => x.ItemInstanceId).HasColumnType("char(36)");
            builder.Property(x => x.UpdatedAt).HasColumnType("datetime(6)").HasDefaultValueSql("CURRENT_TIMESTAMP(6)").IsRequired();

            builder
                .HasIndex(x => x.ItemInstanceId)
                .HasDatabaseName("IX_Shared_ItemStorageCharacterSlot_ItemInstanceId");

            builder
                .HasOne<OwnerItemStorageCharacterReadModel>()
                .WithMany()
                .HasForeignKey(x => new { x.CharacterId, x.Type })
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Shared_ItemStorageCharacterSlot_Shared_ItemStorageCharacter");

            builder
                .HasOne<OwnerItemStorageInstanceReadModel>()
                .WithMany()
                .HasForeignKey(x => x.ItemInstanceId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_Shared_ItemStorageCharacterSlot_Shared_ItemStorageInstance");
        }
    }
}
