using FinVault.IdentityService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinVault.IdentityService.Infrastructure.Persistence.Configurations;

// Creates : RefreshTokens table in finvault_identity database
public class RefreshTokenConfiguration
    : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.HasKey(x => x.Id);

        // Token must be unique
        // No two tokens can be identical
        // This is a security requirement
        builder.Property(x => x.Token)
               .IsRequired()
               .HasMaxLength(512);

        builder.HasIndex(x => x.Token)
               .IsUnique();

        // FOREIGN KEY relationship
        // HasOne = RefreshToken has ONE User
        // WithMany = User has MANY RefreshTokens
        //            (user can login from multiple devices)
        // HasForeignKey = UserId links to Users.Id
        // OnDelete Cascade = if User is deleted
        //                    all their tokens are deleted too
        builder.HasOne(x => x.User)
               .WithMany()
               .HasForeignKey(x => x.UserId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.ToTable("RefreshTokens");
    }
}