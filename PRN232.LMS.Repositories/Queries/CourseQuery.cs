namespace PRN232.LMS.Repositories.Queries;

/// <param name="Search">Matched against the course name.</param>
public record CourseQuery(
    string? Search,
    int? SemesterId,
    int? SubjectId,
    bool IncludeSemester,
    bool IncludeSubject,
    bool IncludeEnrollments);
