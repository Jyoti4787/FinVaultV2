using FinVault.IdentityService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinVault.IdentityService.Infrastructure.Persistence.Configurations;

// Creates : OTPCodes table in finvault_identity database
public class OTPCodeConfiguration
    : IEntityTypeConfiguration<OTPCode>
{
    public void Configure(EntityTypeBuilder<OTPCode> builder)
    {
        builder.HasKey(x => x.Id);

        // CodeHash stores the hashed 6-digit OTP
        // BCrypt hashes are about 60 chars
        // 512 is safe maximum
        builder.Property(x => x.CodeHash)
               .IsRequired()
               .HasMaxLength(512);

        // Purpose tells us WHY this OTP was created
        // "Login" / "Payment" / "PasswordReset"
        // 30 chars is enough
        builder.Property(x => x.Purpose)
               .IsRequired()
               .HasMaxLength(30);

        // Same foreign key pattern as RefreshToken
        // OTPCode belongs to ONE User
        // User has MANY OTPCodes
        // If User deleted → all their OTPs deleted
        builder.HasOne(x => x.User)
               .WithMany()
               .HasForeignKey(x => x.UserId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.ToTable("OTPCodes");
    }
}