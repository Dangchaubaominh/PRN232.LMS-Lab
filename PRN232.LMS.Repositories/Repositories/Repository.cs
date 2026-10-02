using Microsoft.EntityFrameworkCore;
using PRN232.LMS.Repositories.Data;

namespace PRN232.LMS.Repositories.Repositories;

public abstract class Repository<T>(LmsDbContext context) : IRepository<T> where T : class
{
    protected LmsDbContext Context { get; } = context;

    /// <summary>Starting point for read-only queries (no change tracking).</summary>
    protected IQueryable<T> Query => Context.Set<T>().AsNoTracking();

    public Task<T?> FindAsync(int id) => Context.Set<T>().FindAsync(id).AsTask();

    public Task<bool> ExistsAsync(int id)
    {
        var keyName = Context.Model.FindEntityType(typeof(T))!.FindPrimaryKey()!.Properties[0].Name;
        return Query.AnyAsync(x => EF.Property<int>(x, keyName) == id);
    }

    public Task AddAsync(T entity) => Context.Set<T>().AddAsync(entity).AsTask();

    public void Remove(T entity) => Context.Set<T>().Remove(entity);

    public Task SaveChangesAsync() => Context.SaveChangesAsync();
}
