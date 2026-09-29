using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using PRN232.LMS.Services.BusinessModels;
using PRN232.LMS.Services.Exceptions;

namespace PRN232.LMS.Services.Queries;

/// <summary>Sort, paging and expand plumbing shared by every collection query.</summary>
internal static class QueryHelper
{
    public static HashSet<string> SplitList(string? value) =>
        (value ?? string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

    /// <summary>Rejects sort fields and expand values the resource does not support.</summary>
    public static void EnsureSupported<T>(
        ListQuery query,
        IReadOnlyDictionary<string, Expression<Func<T, object>>> sortKeys,
        IReadOnlyCollection<string> expandable)
    {
        var unknownSort = SortTokens(query.Sort)
            .Select(token => token.TrimStart('-'))
            .FirstOrDefault(field => !sortKeys.ContainsKey(field));
        if (unknownSort is not null)
        {
            throw new InvalidQueryException($"Unknown sort field '{unknownSort}'.");
        }

        var unknownExpand = SplitList(query.Expand)
            .FirstOrDefault(name => !expandable.Contains(name, StringComparer.OrdinalIgnoreCase));
        if (unknownExpand is not null)
        {
            throw new InvalidQueryException($"Unknown expand value '{unknownExpand}'.");
        }
    }

    /// <summary>Orders by each "field" or "-field" token in turn, or by <paramref name="fallback"/> when none is given.</summary>
    public static IQueryable<T> ApplySort<T>(
        IQueryable<T> source,
        string? sort,
        IReadOnlyDictionary<string, Expression<Func<T, object>>> sortKeys,
        Expression<Func<T, object>> fallback)
    {
        IOrderedQueryable<T>? ordered = null;
        foreach (var token in SortTokens(sort))
        {
            var descending = token.StartsWith('-');
            if (!sortKeys.TryGetValue(descending ? token[1..] : token, out var key))
            {
                continue;
            }

            ordered = ordered is null
                ? descending ? source.OrderByDescending(key) : source.OrderBy(key)
                : descending ? ordered.ThenByDescending(key) : ordered.ThenBy(key);
        }

        return ordered ?? source.OrderBy(fallback);
    }

    public static async Task<PagedResult<TModel>> ToPagedResultAsync<TEntity, TModel>(
        IQueryable<TEntity> source,
        ListQuery query,
        Func<TEntity, TModel> map)
    {
        var totalItems = await source.CountAsync();
        var rows = await source.Skip((query.Page - 1) * query.Size).Take(query.Size).ToListAsync();
        var totalPages = (int)Math.Ceiling(totalItems / (double)query.Size);

        return new PagedResult<TModel>(rows.Select(map).ToList(), query.Page, query.Size, totalItems, totalPages);
    }

    private static string[] SortTokens(string? sort) =>
        (sort ?? string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
}
