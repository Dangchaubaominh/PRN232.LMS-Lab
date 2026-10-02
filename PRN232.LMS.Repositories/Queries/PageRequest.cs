namespace PRN232.LMS.Repositories.Queries;

/// <summary>Which page to read and in which order. Sort names must be among the repository's SortFields.</summary>
public record PageRequest(int Page, int Size, IReadOnlyList<SortField> Sort);

public record SortField(string Name, bool Descending);

/// <summary>One page of rows plus the number of rows matching the query on all pages.</summary>
public record PagedEntities<T>(IReadOnlyList<T> Items, int TotalItems);
