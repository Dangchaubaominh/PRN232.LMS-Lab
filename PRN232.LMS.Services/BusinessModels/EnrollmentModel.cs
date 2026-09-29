namespace PRN232.LMS.Services.BusinessModels;

public record EnrollmentModel(int EnrollmentId, int StudentId, int CourseId, DateTime EnrollDate, string Status)
{
    /// <summary>Null when the student was not loaded.</summary>
    public StudentModel? Student { get; init; }

    /// <summary>Null when the course was not loaded.</summary>
    public CourseModel? Course { get; init; }
}
