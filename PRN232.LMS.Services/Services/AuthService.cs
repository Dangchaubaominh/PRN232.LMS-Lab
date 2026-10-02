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

        return await StoreAsync(CreateTokens(user, DateTime.UtcNow));
    }

    public async Task<AuthResult> RefreshAsync(string refreshToken)
    {
        var now = DateTime.UtcNow;
        var stored = await userRepository.GetRefreshTokenAsync(HashToken(refreshToken))
            ?? throw new UnauthorizedException(InvalidRefreshToken);

        if (stored.RevokedAt is null && stored.ExpiresAt <= now)
        {
            throw new UnauthorizedException("Refresh token has expired.");
        }

        var tokens = CreateTokens(stored.User, now);
        // TryRevoke is a single conditional UPDATE, so of several requests presenting the same token
        // only one succeeds. Failing here means the token was already used, now or earlier: it may have
        // been stolen, so end every session of the user.
        if (stored.RevokedAt is not null
            || !await userRepository.TryRevokeRefreshTokenAsync(stored.RefreshTokenId, now, tokens.Stored.TokenHash))
        {
            await userRepository.RevokeActiveRefreshTokensAsync(stored.UserId, now);
            throw new UnauthorizedException(InvalidRefreshToken);
        }

        return await StoreAsync(tokens);
    }

    public async Task LogoutAsync(string refreshToken)
    {
        var stored = await userRepository.GetRefreshTokenAsync(HashToken(refreshToken));
        if (stored is not null)
        {
            await userRepository.TryRevokeRefreshTokenAsync(stored.RefreshTokenId, DateTime.UtcNow);
        }
    }

    /// <summary>A new access token and refresh token; the refresh token is not saved yet.</summary>
    private (AuthResult Result, RefreshToken Stored) CreateTokens(User user, DateTime now)
    {
        var accessToken = tokenService.CreateAccessToken(user.ToModel());
        var refreshToken = tokenService.CreateRefreshToken();
        var stored = new RefreshToken
        {
            UserId = user.UserId,
            TokenHash = HashToken(refreshToken.Token),
            CreatedAt = now,
            ExpiresAt = refreshToken.ExpiresAt
        };

        return (new AuthResult(accessToken.Token, refreshToken.Token, accessToken.ExpiresInSeconds), stored);
    }

    private async Task<AuthResult> StoreAsync((AuthResult Result, RefreshToken Stored) tokens)
    {
        await userRepository.AddRefreshTokenAsync(tokens.Stored);
        await userRepository.SaveChangesAsync();
        return tokens.Result;
    }

    private static string HashToken(string token) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
}
