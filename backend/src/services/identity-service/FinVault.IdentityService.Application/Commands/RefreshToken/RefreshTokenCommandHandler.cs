using FinVault.IdentityService.Domain.Entities;
using FinVault.IdentityService.Domain.Interfaces;
using MediatR;
using RefreshTokenEntity = FinVault.IdentityService.Domain.Entities.RefreshToken;

namespace FinVault.IdentityService.Application.Commands.RefreshToken;

public class RefreshTokenCommandHandler
    : IRequestHandler<RefreshTokenCommand, RefreshTokenResult>
{
    private readonly IRefreshTokenRepository _tokens;
    private readonly IUserRepository _users;
    private readonly IJwtTokenService _jwtService;

    public RefreshTokenCommandHandler(
        IRefreshTokenRepository tokens,
        IUserRepository users,
        IJwtTokenService jwtService)
    {
        _tokens     = tokens;
        _users      = users;
        _jwtService = jwtService;
    }

    public async Task<RefreshTokenResult> Handle(
        RefreshTokenCommand cmd,
        CancellationToken ct)
    {
        var existing = await _tokens.GetByTokenAsync(
            cmd.Token, ct)
            ?? throw new UnauthorizedAccessException(
                "Invalid refresh token.");

        if (existing.IsRevoked ||
            existing.ExpiresAt < DateTimeOffset.UtcNow)
            throw new UnauthorizedAccessException(
                "Refresh token expired or revoked.");

        var user = await _users.GetByIdAsync(
            existing.UserId, ct)
            ?? throw new UnauthorizedAccessException(
                "User not found.");

        await _tokens.RevokeAsync(existing.Id, ct);

        var newAccess  = _jwtService.GenerateAccessToken(user);
        var newRefresh = _jwtService.GenerateRefreshToken();
        var expiresAt  = DateTimeOffset.UtcNow.AddMinutes(15);

        await _tokens.AddAsync(new RefreshTokenEntity
        {
            Id        = Guid.NewGuid(),
            UserId    = user.Id,
            Token     = newRefresh,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(7),
            CreatedAt = DateTimeOffset.UtcNow
        }, ct);

        return new RefreshTokenResult(
            newAccess, newRefresh, expiresAt);
    }
}