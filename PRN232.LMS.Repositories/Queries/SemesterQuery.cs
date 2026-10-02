namespace PRN232.LMS.Repositories.Queries;

/// <param name="Search">Matched against the semester name.</param>
/// <param name="IncludeCourses">Load each semester's courses.</param>
public record SemesterQuery(string? Search, bool IncludeCourses);
