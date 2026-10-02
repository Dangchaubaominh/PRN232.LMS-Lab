using Microsoft.EntityFrameworkCore;
using PRN232.LMS.Repositories.Data;
using PRN232.LMS.Repositories.Entities;

namespace PRN232.LMS.Repositories.Repositories;

public class UserRepository(LmsDbContext context) : IUserRepository
{
    public Task<User?> GetByUsernameAsync(string username) =>
        context.Users.AsNoTracking().SingleOrDefaultAsync(x => x.Username == username);

    public Task<RefreshToken?> GetRefreshTokenAsync(string tokenHash) =>
        context.RefreshTokens.AsNoTracking().Include(x => x.User).SingleOrDefaultAsync(x => x.TokenHash == tokenHash);

    public async Task<bool> TryRevokeRefreshTokenAsync(int refreshTokenId, DateTime now, string? replacedByTokenHash = null) =>
        await context.RefreshTokens
            .Where(x => x.RefreshTokenId == refreshTokenId && x.RevokedAt == null)
            .ExecuteUpdateAsync(s => s
                .SetProperty(x => x.RevokedAt, now)
                .SetProperty(x => x.ReplacedByTokenHash, replacedByTokenHash)) == 1;

    public Task RevokeActiveRefreshTokensAsync(int userId, DateTime now) =>
        context.RefreshTokens
            .Where(x => x.UserId == userId && x.RevokedAt == null)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.RevokedAt, now));

    public Task AddRefreshTokenAsync(RefreshToken token) => context.RefreshTokens.AddAsync(token).AsTask();

    public Task SaveChangesAsync() => context.SaveChangesAsync();
}
