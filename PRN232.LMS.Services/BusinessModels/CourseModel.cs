namespace PRN232.LMS.Services.BusinessModels;

public record CourseModel(int CourseId, string CourseName, int SemesterId, int SubjectId)
{
    /// <summary>Null when the semester was not loaded.</summary>
    public SemesterModel? Semester { get; init; }

    /// <summary>Null when the subject was not loaded.</summary>
    public SubjectModel? Subject { get; init; }

    /// <summary>Null when the enrollments were not loaded.</summary>
    public IReadOnlyList<EnrollmentModel>? Enrollments { get; init; }
}
