using DigitalWorldOnline.Infraestructure.Repositories.Shared.ReadModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DigitalWorldOnline.Infraestructure.ContextConfiguration.Shared
{
    public sealed class OwnerItemStorageCharacterConfiguration : IEntityTypeConfiguration<OwnerItemStorageCharacterReadModel>
    {
        public void Configure(EntityTypeBuilder<OwnerItemStorageCharacterReadModel> builder)
        {
            builder
                .ToTable("Shared_ItemStorageCharacter")
                .HasKey(x => new { x.CharacterId, x.Type });

            builder.Property(x => x.CharacterId).HasColumnType("bigint").IsRequired();
            builder.Property(x => x.Type).HasColumnType("int").IsRequired();
            builder.Property(x => x.Size).HasColumnType("smallint unsigned").IsRequired();
            builder.Property(x => x.Bits).HasColumnType("bigint").HasDefaultValue(0).IsRequired();
            builder.Property(x => x.CreatedAt).HasColumnType("datetime(6)").HasDefaultValueSql("CURRENT_TIMESTAMP(6)").IsRequired();
            builder.Property(x => x.UpdatedAt).HasColumnType("datetime(6)").HasDefaultValueSql("CURRENT_TIMESTAMP(6)").IsRequired();

            builder
                .HasOne<DigitalWorldOnline.Commons.DTOs.Character.CharacterDTO>()
                .WithMany()
                .HasForeignKey(x => x.CharacterId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Shared_ItemStorageCharacter_Character_CharacterId");
        }
    }
}
