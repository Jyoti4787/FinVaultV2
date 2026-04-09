// ==================================================================
// FILE : ProfilePictureRepository.cs
// LAYER: Infrastructure (Persistence)
// WHAT IS THIS?
// Stores profile picture binary data in SQL Server (varbinary).
// No MongoDB required — pure SQL-only architecture.
// ==================================================================

using FinVault.IdentityService.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinVault.IdentityService.Infrastructure.Persistence;

public class ProfilePictureRepository : IProfilePictureRepository
{
    private readonly IdentityDbContext _db;
    private readonly string _baseUrl;

    public ProfilePictureRepository(IdentityDbContext db, Microsoft.Extensions.Configuration.IConfiguration config)
    {
        _db      = db;
        _baseUrl = config["ProfilePictures:BaseUrl"] ?? "http://localhost:5232";
    }

    public async Task<string> UploadAsync(
        Guid userId, string fileName, string contentType,
        Stream imageStream, CancellationToken ct = default)
    {
        using var ms = new MemoryStream();
        await imageStream.CopyToAsync(ms, ct);
        var bytes = ms.ToArray();

        var user = await _db.Users.FindAsync(new object[] { userId }, ct)
            ?? throw new KeyNotFoundException("User not found.");

        user.ProfilePictureData        = bytes;
        user.ProfilePictureContentType = contentType;
        user.ProfilePictureUrl         = $"{_baseUrl}/api/identity/users/profile/picture/{userId}";
        user.UpdatedAt                 = DateTimeOffset.UtcNow;

        await _db.SaveChangesAsync(ct);
        return user.ProfilePictureUrl;
    }

    public async Task<(Stream stream, string contentType)> DownloadAsync(
        string userId, CancellationToken ct = default)
    {
        if (!Guid.TryParse(userId, out var id))
            throw new FileNotFoundException("Invalid user ID.");

        var user = await _db.Users
            .Where(u => u.Id == id && u.ProfilePictureData != null)
            .Select(u => new { u.ProfilePictureData, u.ProfilePictureContentType })
            .FirstOrDefaultAsync(ct)
            ?? throw new FileNotFoundException("Profile picture not found.");

        return (new MemoryStream(user.ProfilePictureData!), user.ProfilePictureContentType ?? "image/jpeg");
    }

    public async Task DeleteByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _db.Users.FindAsync(new object[] { userId }, ct);
        if (user is null) return;
        user.ProfilePictureData        = null;
        user.ProfilePictureContentType = null;
        user.ProfilePictureUrl         = null;
        await _db.SaveChangesAsync(ct);
    }
}
