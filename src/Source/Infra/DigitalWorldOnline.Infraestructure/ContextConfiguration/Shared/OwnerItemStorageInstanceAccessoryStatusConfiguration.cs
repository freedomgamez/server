using DigitalWorldOnline.Infraestructure.Repositories.Shared.ReadModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DigitalWorldOnline.Infraestructure.ContextConfiguration.Shared
{
    public sealed class OwnerItemStorageInstanceAccessoryStatusConfiguration : IEntityTypeConfiguration<OwnerItemStorageInstanceAccessoryStatusReadModel>
    {
        public void Configure(EntityTypeBuilder<OwnerItemStorageInstanceAccessoryStatusReadModel> builder)
        {
            builder
                .ToTable("Shared_ItemStorageInstanceAccessoryStatus")
                .HasKey(x => x.Id);

            builder.Property(x => x.ItemInstanceId).HasColumnType("char(36)").IsRequired();
            builder.Property(x => x.Slot).HasColumnType("tinyint unsigned").IsRequired();
            builder.Property(x => x.Type).HasColumnType("smallint").IsRequired();
            builder.Property(x => x.Value).HasColumnType("smallint").IsRequired();

            builder
                .HasIndex(x => new { x.ItemInstanceId, x.Slot })
                .IsUnique()
                .HasDatabaseName("UX_Shared_ItemStorageInstanceAccessoryStatus_Instance_Slot");

            builder
                .HasOne<OwnerItemStorageInstanceReadModel>()
                .WithMany()
                .HasForeignKey(x => x.ItemInstanceId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Shared_ItemStorageInstanceAccessoryStatus_Shared_ItemStorageInstance");
        }
    }
}
