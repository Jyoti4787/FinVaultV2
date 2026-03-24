using FinVault.IdentityService.Domain.Interfaces;
using MediatR;
using RefreshTokenEntity = FinVault.IdentityService.Domain.Entities.RefreshToken;

namespace FinVault.IdentityService.Application.Commands.LoginUser;

public class LoginUserCommandHandler
    : IRequestHandler<LoginUserCommand, LoginUserResult>
{
    private readonly IUserRepository _users;
    private readonly IRefreshTokenRepository _tokens;
    private readonly IJwtTokenService _jwtService;

    public LoginUserCommandHandler(
        IUserRepository users,
        IRefreshTokenRepository tokens,
        IJwtTokenService jwtService)
    {
        _users      = users;
        _tokens     = tokens;
        _jwtService = jwtService;
    }

    public async Task<LoginUserResult> Handle(
        LoginUserCommand cmd,
        CancellationToken ct)
    {
        var user = await _users.GetByEmailAsync(
            cmd.Email.ToLowerInvariant(), ct)
            ?? throw new UnauthorizedAccessException(
                "Invalid email or password.");

        if (!BCrypt.Net.BCrypt.Verify(
            cmd.Password, user.PasswordHash))
            throw new UnauthorizedAccessException(
                "Invalid email or password.");

        if (!user.IsActive)
            throw new UnauthorizedAccessException(
                "Account is disabled.");

        var accessToken  = _jwtService.GenerateAccessToken(user);
        var refreshToken = _jwtService.GenerateRefreshToken();
        var expiresAt    = DateTimeOffset.UtcNow.AddMinutes(15);

        await _tokens.AddAsync(new RefreshTokenEntity
        {
            Id        = Guid.NewGuid(),
            UserId    = user.Id,
            Token     = refreshToken,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(7),
            CreatedAt = DateTimeOffset.UtcNow
        }, ct);

        return new LoginUserResult(
            accessToken,
            refreshToken,
            user.Id,
            user.Email,
            user.Role,
            expiresAt);
    }
}
