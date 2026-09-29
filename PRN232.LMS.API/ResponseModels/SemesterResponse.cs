using System.Text.Json.Serialization;

namespace PRN232.LMS.API.ResponseModels;

public class SemesterResponse
{
    public int SemesterId { get; init; }
    public string SemesterName { get; init; } = string.Empty;
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<CourseResponse>? Courses { get; init; }
}
