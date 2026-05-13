using DigitalWorldOnline.Infraestructure.Repositories.Shared.ReadModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DigitalWorldOnline.Infraestructure.ContextConfiguration.Shared
{
    public sealed class OwnerItemStorageAccountConfiguration : IEntityTypeConfiguration<OwnerItemStorageAccountReadModel>
    {
        public void Configure(EntityTypeBuilder<OwnerItemStorageAccountReadModel> builder)
        {
            builder
                .ToTable("Shared_ItemStorageAccount")
                .HasKey(x => new { x.AccountId, x.Type });

            builder.Property(x => x.AccountId).HasColumnType("bigint").IsRequired();
            builder.Property(x => x.Type).HasColumnType("int").IsRequired();
            builder.Property(x => x.Size).HasColumnType("smallint unsigned").IsRequired();
            builder.Property(x => x.Bits).HasColumnType("bigint").HasDefaultValue(0).IsRequired();
            builder.Property(x => x.CreatedAt).HasColumnType("datetime(6)").HasDefaultValueSql("CURRENT_TIMESTAMP(6)").IsRequired();
            builder.Property(x => x.UpdatedAt).HasColumnType("datetime(6)").HasDefaultValueSql("CURRENT_TIMESTAMP(6)").IsRequired();

            builder
                .HasOne<DigitalWorldOnline.Commons.DTOs.Account.AccountDTO>()
                .WithMany()
                .HasForeignKey(x => x.AccountId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Shared_ItemStorageAccount_Account_AccountId");
        }
    }
}
