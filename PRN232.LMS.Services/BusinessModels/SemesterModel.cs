namespace PRN232.LMS.Services.BusinessModels;

public record SemesterModel(int SemesterId, string SemesterName, DateTime StartDate, DateTime EndDate)
{
    /// <summary>Null when the courses were not loaded.</summary>
    public IReadOnlyList<CourseModel>? Courses { get; init; }
}
