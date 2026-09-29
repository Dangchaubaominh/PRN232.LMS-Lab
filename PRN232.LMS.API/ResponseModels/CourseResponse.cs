using System.Text.Json.Serialization;

namespace PRN232.LMS.API.ResponseModels;

public class CourseResponse
{
    public int CourseId { get; init; }
    public string CourseName { get; init; } = string.Empty;
    public int SemesterId { get; init; }
    public int SubjectId { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public SemesterResponse? Semester { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public SubjectResponse? Subject { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<EnrollmentResponse>? Enrollments { get; init; }
}
