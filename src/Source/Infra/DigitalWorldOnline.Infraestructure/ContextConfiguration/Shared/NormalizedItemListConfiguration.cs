using DigitalWorldOnline.Infraestructure.Repositories.Shared.ReadModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DigitalWorldOnline.Infraestructure.ContextConfiguration.Shared
{
    public class NormalizedItemListConfiguration : IEntityTypeConfiguration<NormalizedItemListReadModel>
    {
        public void Configure(EntityTypeBuilder<NormalizedItemListReadModel> builder)
        {
            builder
                .ToTable("Shared_ItemListNormalized", tableBuilder =>
                {
                    tableBuilder.HasCheckConstraint(
                        "CK_Shared_ItemListNormalized_ExactlyOneOwner",
                        "((`CharacterId` IS NOT NULL AND `AccountId` IS NULL) OR (`CharacterId` IS NULL AND `AccountId` IS NOT NULL))");
                })
                .HasKey(x => x.Id);

            builder
                .Property(x => x.Id)
                .ValueGeneratedOnAdd();

            builder
                .Property(x => x.Type)
                .HasColumnType("int")
                .IsRequired();

            builder
                .Property(x => x.Size)
                .HasColumnType("smallint unsigned")
                .IsRequired();

            builder
                .Property(x => x.Bits)
                .HasColumnType("bigint")
                .HasDefaultValue(0)
                .IsRequired();

            builder
                .Property(x => x.CreatedAt)
                .HasColumnType("datetime(6)")
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)")
                .IsRequired();

            builder
                .Property(x => x.UpdatedAt)
                .HasColumnType("datetime(6)")
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)")
                .IsRequired();

            builder
                .HasIndex(x => new { x.CharacterId, x.Type })
                .IsUnique()
                .HasDatabaseName("IX_Shared_ItemListNormalized_Character_Type");

            builder
                .HasIndex(x => new { x.AccountId, x.Type })
                .IsUnique()
                .HasDatabaseName("IX_Shared_ItemListNormalized_Account_Type");
        }
    }
}
