using PRN232.LMS.Repositories.Entities;

namespace PRN232.LMS.Repositories.Repositories;

/// <summary>Data access for users and their refresh tokens.</summary>
public interface IUserRepository
{
    Task<User?> GetByUsernameAsync(string username);

    /// <summary>Read-only, with its user loaded.</summary>
    Task<RefreshToken?> GetRefreshTokenAsync(string tokenHash);

    /// <summary>
    /// Revokes the token only if it is still active, in a single UPDATE. When several requests try
    /// to revoke the same token at once, exactly one of them gets true.
    /// </summary>
    Task<bool> TryRevokeRefreshTokenAsync(int refreshTokenId, DateTime now, string? replacedByTokenHash = null);

    /// <summary>Revokes every refresh token of the user that is not revoked yet.</summary>
    Task RevokeActiveRefreshTokensAsync(int userId, DateTime now);

    Task AddRefreshTokenAsync(RefreshToken token);
    Task SaveChangesAsync();
}
