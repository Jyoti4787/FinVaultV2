namespace FinVault.IdentityService.Domain.Interfaces;

public interface IProfilePictureRepository
{
    Task<string> UploadAsync(Guid userId, string fileName, string contentType, Stream imageStream, CancellationToken ct = default);
    Task<(Stream stream, string contentType)> DownloadAsync(string fileId, CancellationToken ct = default);
    Task DeleteByUserIdAsync(Guid userId, CancellationToken ct = default);
}
