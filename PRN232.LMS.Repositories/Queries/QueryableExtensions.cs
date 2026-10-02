using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace PRN232.LMS.Repositories.Queries;

/// <summary>Sorting and paging shared by the repositories.</summary>
internal static class QueryableExtensions
{
    /// <summary>Orders by each sort field in turn, or by <paramref name="fallback"/> when none is given.</summary>
    public static IQueryable<T> ApplySort<T>(
        this IQueryable<T> source,
        IReadOnlyList<SortField> sort,
        IReadOnlyDictionary<string, Expression<Func<T, object>>> keys,
        Expression<Func<T, object>> fallback)
    {
        IOrderedQueryable<T>? ordered = null;
        foreach (var field in sort)
        {
            // Names are validated by the service layer against SortFields; anything else is ignored here.
            if (!keys.TryGetValue(field.Name, out var key))
            {
                continue;
            }

            ordered = ordered is null
                ? field.Descending ? source.OrderByDescending(key) : source.OrderBy(key)
                : field.Descending ? ordered.ThenByDescending(key) : ordered.ThenBy(key);
        }

        return ordered ?? source.OrderBy(fallback);
    }

    public static async Task<PagedEntities<T>> ToPageAsync<T>(this IQueryable<T> source, PageRequest page)
    {
        var totalItems = await source.CountAsync();
        // long: (Page - 1) * Size overflows int for very large page numbers.
        var skip = (long)(page.Page - 1) * page.Size;
        var items = skip < totalItems
            ? await source.Skip((int)skip).Take(page.Size).ToListAsync()
            : [];

        return new PagedEntities<T>(items, totalItems);
    }
}
