using DigitalWorldOnline.Commons.Enums.Account;
using DigitalWorldOnline.Commons.DTOs.Account;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace DigitalWorldOnline.Infraestructure.ContextConfiguration.Account
{
    public class AccountConfiguration : IEntityTypeConfiguration<AccountDTO>
    {
        public void Configure(EntityTypeBuilder<AccountDTO> builder)
        {
            builder
                .ToTable("Account_Account")
                .HasKey(x => x.Id);

            builder
                .Property(x => x.Username)
                .HasColumnType("varchar(255)")
                .HasMaxLength(20)
                .IsRequired();

            builder
                .Property(x => x.Password)
                .HasColumnType("varchar(255)")
                .HasMaxLength(35)
                .IsRequired();

            builder
                .Property(x => x.SecondaryPassword)
                .HasColumnType("varchar(255)")
                .HasMaxLength(250);

            builder
                .Property(x => x.Email)
                .HasColumnType("varchar(255)")
                .HasMaxLength(150)
                .IsRequired();
            
            builder
                .Property(x => x.DiscordId)
                .HasColumnType("varchar(255)")
                .HasMaxLength(100)
                .IsRequired();

            builder
                .Property(x => x.AccessLevel)
                .HasConversion(new ValueConverter<AccountAccessLevelEnum, int>(
                    x => (int)x,
                    x => (AccountAccessLevelEnum)x))
                .HasColumnType("int")
                .HasDefaultValue(AccountAccessLevelEnum.Default)
                .IsRequired();

            builder
                .Property(x => x.CreateDate)
                .HasColumnType("datetime(6)")
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)")
                .IsRequired();

            builder
                .Property(x => x.LastConnection)
                .HasColumnType("datetime(6)");

            builder
                .Property(x => x.MembershipExpirationDate)
                .HasColumnType("datetime(6)");

            builder
                .Property(x => x.Premium)
                .HasColumnType("int")
                .HasDefaultValue(0)
                .IsRequired();

            builder
                .Property(x => x.Silk)
                .HasColumnType("int")
                .HasDefaultValue(0)
                .IsRequired();

            builder
                .Property(x => x.LastPlayedServer)
                .HasColumnType("bigint")
                .HasDefaultValue(0)
                .IsRequired();

            builder
                .Property(x => x.LastPlayedCharacter)
                .HasColumnType("bigint")
                .HasDefaultValue(0)
                .IsRequired();

            builder
                .Property(x => x.ReceiveWelcome)
                .HasColumnType("tinyint(1)")
                .IsRequired();

            builder
                .HasOne(x => x.SystemInformation)
                .WithOne(x => x.Account)
                .HasForeignKey<SystemInformationDTO>(x => x.AccountId);

            builder
                .HasOne(x => x.AccountBlock)
                .WithOne(x => x.Account)
                .HasForeignKey<AccountBlockDTO>(x => x.AccountId);
        }
    }
}
