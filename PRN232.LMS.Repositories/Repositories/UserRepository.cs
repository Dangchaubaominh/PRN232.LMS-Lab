using Microsoft.EntityFrameworkCore;
using PRN232.LMS.Repositories.Data;
using PRN232.LMS.Repositories.Entities;

namespace PRN232.LMS.Repositories.Repositories;

public class UserRepository(LmsDbContext context) : IUserRepository
{
    public Task<User?> GetByUsernameAsync(string username) =>
        context.Users.AsNoTracking().SingleOrDefaultAsync(x => x.Username == username);

    public Task<RefreshToken?> GetRefreshTokenAsync(string tokenHash) =>
        context.RefreshTokens.Include(x => x.User).SingleOrDefaultAsync(x => x.TokenHash == tokenHash);

    public Task<List<RefreshToken>> GetActiveRefreshTokensAsync(int userId, DateTime now) =>
        context.RefreshTokens.Where(x => x.UserId == userId && x.RevokedAt == null && x.ExpiresAt > now).ToListAsync();

    public Task AddRefreshTokenAsync(RefreshToken token) => context.RefreshTokens.AddAsync(token).AsTask();

    public Task SaveChangesAsync() => context.SaveChangesAsync();
}
