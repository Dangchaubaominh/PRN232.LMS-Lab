using System.Text.Json.Serialization;

namespace PRN232.LMS.API.ResponseModels;

public class SubjectResponse
{
    public int SubjectId { get; init; }
    public string SubjectCode { get; init; } = string.Empty;
    public string SubjectName { get; init; } = string.Empty;
    public int Credit { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<CourseResponse>? Courses { get; init; }
}
