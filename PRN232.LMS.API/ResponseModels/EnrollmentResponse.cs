using System.Text.Json.Serialization;

namespace PRN232.LMS.API.ResponseModels;

public class EnrollmentResponse
{
    public int EnrollmentId { get; init; }
    public int StudentId { get; init; }
    public int CourseId { get; init; }
    public DateTime EnrollDate { get; init; }
    public string Status { get; init; } = string.Empty;

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public StudentResponse? Student { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public CourseResponse? Course { get; init; }
}
