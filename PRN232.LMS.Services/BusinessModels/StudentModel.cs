namespace PRN232.LMS.Services.BusinessModels;

public record StudentModel(int StudentId, string StudentCode, string FullName, string Email, string? Phone, DateTime DateOfBirth)
{
    /// <summary>Null when the enrollments were not loaded.</summary>
    public IReadOnlyList<EnrollmentModel>? Enrollments { get; init; }
}
