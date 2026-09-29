namespace PRN232.LMS.Services.BusinessModels;

/// <summary>
/// Search, filter, sort, paging and expansion options for a collection. Field selection is not
/// part of it: choosing which properties to return is done by the API layer.
/// </summary>
public class ListQuery
{
    public string? Search { get; init; }
    public string? Status { get; init; }
    public int? SemesterId { get; init; }
    public int? SubjectId { get; init; }
    public int? StudentId { get; init; }
    public int? CourseId { get; init; }

    /// <summary>Comma separated field names; a leading '-' sorts that field descending.</summary>
    public string? Sort { get; init; }

    public int Page { get; init; } = 1;
    public int Size { get; init; } = 10;

    /// <summary>Comma separated names of related resources to load.</summary>
    public string? Expand { get; init; }
}
