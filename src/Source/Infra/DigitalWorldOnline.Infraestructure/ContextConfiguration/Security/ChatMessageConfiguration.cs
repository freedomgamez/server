using DigitalWorldOnline.Commons.DTOs.Chat;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DigitalWorldOnline.Infraestructure.ContextConfiguration.Security
{
    public class ChatMessageConfiguration : IEntityTypeConfiguration<ChatMessageDTO>
    {
        public void Configure(EntityTypeBuilder<ChatMessageDTO> builder)
        {
            builder
                .ToTable("Security_ChatMessage")
                .HasKey(x => x.Id);

            builder
                .Property(x => x.Time)
                .HasColumnType("datetime(6)")
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)")
                .IsRequired();

            builder
                .Property(x => x.Message)
                .HasColumnType("varchar(255)")
                .HasMaxLength(200)
                .IsRequired();

            builder
                .HasOne(x => x.Character);
        }
    }
}