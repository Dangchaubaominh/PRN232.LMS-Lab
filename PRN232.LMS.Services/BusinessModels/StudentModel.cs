namespace PRN232.LMS.Services.BusinessModels;

public record StudentModel(int StudentId, string FullName, string Email, DateTime DateOfBirth)
{
    /// <summary>Null when the enrollments were not loaded.</summary>
    public IReadOnlyList<EnrollmentModel>? Enrollments { get; init; }
}
