using PRN232.LMS.Repositories.Queries;

namespace PRN232.LMS.Repositories.Repositories;

/// <summary>Operations every entity repository offers.</summary>
public interface IRepository<T> where T : class
{
    /// <summary>Tracked, so changes are saved by <see cref="SaveChangesAsync"/>.</summary>
    Task<T?> FindAsync(int id);

    Task<bool> ExistsAsync(int id);
    Task AddAsync(T entity);
    void Remove(T entity);

    /// <summary>Saves all pending changes of the request (the repositories share one DbContext).</summary>
    Task SaveChangesAsync();
}

/// <summary>A repository with a filtered, sorted, paged list.</summary>
public interface IPagedRepository<T, in TQuery> : IRepository<T> where T : class
{
    /// <summary>Names accepted in <see cref="PageRequest.Sort"/>, case-insensitive.</summary>
    IReadOnlyCollection<string> SortFields { get; }

    /// <summary>Read-only entities of one page.</summary>
    Task<PagedEntities<T>> GetPageAsync(TQuery query, PageRequest page);
}
