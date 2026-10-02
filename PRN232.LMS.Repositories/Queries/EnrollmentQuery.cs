namespace PRN232.LMS.Repositories.Queries;

/// <param name="Search">Matched against the enrollment status.</param>
public record EnrollmentQuery(
    string? Search,
    string? Status,
    int? StudentId,
    int? CourseId,
    bool IncludeStudent,
    bool IncludeCourse);
