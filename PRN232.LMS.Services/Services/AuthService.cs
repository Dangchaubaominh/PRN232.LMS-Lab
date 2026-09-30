using System.Security.Cryptography;
using System.Text;
using PRN232.LMS.Repositories.Entities;
using PRN232.LMS.Repositories.Repositories;
using PRN232.LMS.Services.BusinessModels;
using PRN232.LMS.Services.Exceptions;
using PRN232.LMS.Services.Mappings;
using PRN232.LMS.Services.Security;

namespace PRN232.LMS.Services.Services;

public class AuthService(IUserRepository userRepository, IPasswordHasher passwordHasher, ITokenService tokenService) : IAuthService
{
    private const string InvalidCredentials = "Invalid username or password.";
    private const string InvalidRefreshToken = "Invalid refresh token.";

    /// <summary>Hash checked when the username does not exist, so that case takes as long as a wrong password.</summary>
    private static string? _dummyPasswordHash;

    public async Task<AuthResult> LoginAsync(string username, string password)
    {
        var user = await userRepository.GetByUsernameAsync(username.Trim());
        _dummyPasswordHash ??= passwordHasher.Hash(Guid.NewGuid().ToString());

        var passwordMatches = passwordHasher.Verify(password, user?.PasswordHash ?? _dummyPasswordHash);
        if (user is null || !passwordMatches)
        {
            throw new UnauthorizedException(InvalidCredentials);
        }

        return await IssueTokensAsync(user, DateTime.UtcNow);
    }

    public async Task<AuthResult> RefreshAsync(string refreshToken)
    {
        var now = DateTime.UtcNow;
        var stored = await userRepository.GetRefreshTokenAsync(HashToken(refreshToken))
            ?? throw new UnauthorizedException(InvalidRefreshToken);

        if (stored.RevokedAt is not null)
        {
            // An already used token came back: it may have been stolen, so end every session of the user.
            foreach (var active in await userRepository.GetActiveRefreshTokensAsync(stored.UserId, now))
            {
                active.RevokedAt = now;
            }
            await userRepository.SaveChangesAsync();
            throw new UnauthorizedException(InvalidRefreshToken);
        }

        if (stored.ExpiresAt <= now)
        {
            throw new UnauthorizedException("Refresh token has expired.");
        }

        return await IssueTokensAsync(stored.User, now, replacing: stored);
    }

    public async Task LogoutAsync(string refreshToken)
    {
        var stored = await userRepository.GetRefreshTokenAsync(HashToken(refreshToken));
        if (stored is { RevokedAt: null })
        {
            stored.RevokedAt = DateTime.UtcNow;
            await userRepository.SaveChangesAsync();
        }
    }

    /// <summary>Creates an access token and a stored refresh token; revokes <paramref name="replacing"/> (rotation).</summary>
    private async Task<AuthResult> IssueTokensAsync(User user, DateTime now, RefreshToken? replacing = null)
    {
        var accessToken = tokenService.CreateAccessToken(user.ToModel());
        var refreshToken = tokenService.CreateRefreshToken();
        var refreshTokenHash = HashToken(refreshToken.Token);

        await userRepository.AddRefreshTokenAsync(new RefreshToken
        {
            UserId = user.UserId,
            TokenHash = refreshTokenHash,
            CreatedAt = now,
            ExpiresAt = refreshToken.ExpiresAt
        });
        if (replacing is not null)
        {
            replacing.RevokedAt = now;
            replacing.ReplacedByTokenHash = refreshTokenHash;
        }
        await userRepository.SaveChangesAsync();

        return new AuthResult(accessToken.Token, refreshToken.Token, accessToken.ExpiresInSeconds);
    }

    private static string HashToken(string token) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
}
