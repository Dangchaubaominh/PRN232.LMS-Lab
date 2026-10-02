using PRN232.LMS.Repositories.Queries;
using PRN232.LMS.Services.BusinessModels;
using PRN232.LMS.Services.Exceptions;

namespace PRN232.LMS.Services.Queries;

/// <summary>Turns the client's sort/paging/expand options into a repository request, rejecting unsupported values.</summary>
internal static class QueryOptions
{
    public static HashSet<string> SplitList(string? value) =>
        (value ?? string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Parses "field,-field" sort tokens and checks them against <paramref name="sortFields"/>, and
    /// checks every expand value against <paramref name="expandable"/>.
    /// </summary>
    /// <exception cref="InvalidQueryException">A sort field or expand value is not supported.</exception>
    public static PageRequest ToPageRequest(ListQuery query, IReadOnlyCollection<string> sortFields, IReadOnlyCollection<string> expandable)
    {
        var sort = (query.Sort ?? string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(token => token.StartsWith('-') ? new SortField(token[1..], Descending: true) : new SortField(token, Descending: false))
            .ToList();

        var unknownSort = sort.FirstOrDefault(field => !sortFields.Contains(field.Name, StringComparer.OrdinalIgnoreCase));
        if (unknownSort is not null)
        {
            throw new InvalidQueryException($"Unknown sort field '{unknownSort.Name}'.");
        }

        var unknownExpand = SplitList(query.Expand).FirstOrDefault(name => !expandable.Contains(name, StringComparer.OrdinalIgnoreCase));
        if (unknownExpand is not null)
        {
            throw new InvalidQueryException($"Unknown expand value '{unknownExpand}'.");
        }

        return new PageRequest(query.Page, query.Size, sort);
    }

    public static PagedResult<TModel> ToPagedResult<TEntity, TModel>(this PagedEntities<TEntity> page, PageRequest request, Func<TEntity, TModel> map)
    {
        var totalPages = (int)Math.Ceiling(page.TotalItems / (double)request.Size);
        return new PagedResult<TModel>(page.Items.Select(map).ToList(), request.Page, request.Size, page.TotalItems, totalPages);
    }
}
