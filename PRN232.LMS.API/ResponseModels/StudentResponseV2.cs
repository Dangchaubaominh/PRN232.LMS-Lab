namespace PRN232.LMS.API.ResponseModels;

/// <summary>
/// Student representation for API v2. Differences from v1 <see cref="StudentResponse"/>:
/// dateOfBirth is a date ("2001-02-02") instead of a date-time, and enrollmentCount replaces the
/// embedded enrollments list.
/// </summary>
public class StudentResponseV2
{
    public int StudentId { get; init; }
    public string StudentCode { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string? Phone { get; init; }
    public DateOnly DateOfBirth { get; init; }
    public int EnrollmentCount { get; init; }
}
