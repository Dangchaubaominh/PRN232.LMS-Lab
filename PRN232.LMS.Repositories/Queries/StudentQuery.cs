namespace PRN232.LMS.Repositories.Queries;

/// <param name="Search">Matched against student code, full name and email.</param>
/// <param name="IncludeEnrollments">Load each student's enrollments.</param>
/// <param name="EnrolledInCourseId">Keep only students enrolled in this course.</param>
/// <param name="EnrollmentStatus">With <paramref name="EnrolledInCourseId"/>: keep only enrollments with this status.</param>
public record StudentQuery(
    string? Search,
    bool IncludeEnrollments,
    int? EnrolledInCourseId = null,
    string? EnrollmentStatus = null);
