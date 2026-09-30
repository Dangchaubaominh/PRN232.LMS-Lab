using PRN232.LMS.Repositories.Entities;

namespace PRN232.LMS.Repositories.Repositories;

/// <summary>Data access for users and their refresh tokens.</summary>
public interface IUserRepository
{
    Task<User?> GetByUsernameAsync(string username);

    /// <summary>Tracked (so it can be revoked), with its user loaded.</summary>
    Task<RefreshToken?> GetRefreshTokenAsync(string tokenHash);

    /// <summary>Tracked tokens of the user that are neither revoked nor expired at <paramref name="now"/>.</summary>
    Task<List<RefreshToken>> GetActiveRefreshTokensAsync(int userId, DateTime now);

    Task AddRefreshTokenAsync(RefreshToken token);
    Task SaveChangesAsync();
}
