namespace PRN232.LMS.Services.BusinessModels;

public record SubjectModel(int SubjectId, string SubjectCode, string SubjectName, int Credit)
{
    /// <summary>Null when the courses were not loaded.</summary>
    public IReadOnlyList<CourseModel>? Courses { get; init; }
}
