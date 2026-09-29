using System.Text.Json.Serialization;

namespace PRN232.LMS.API.ResponseModels;

public class StudentResponse
{
    public int StudentId { get; init; }
    public string StudentCode { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string? Phone { get; init; }
    public DateTime DateOfBirth { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<EnrollmentResponse>? Enrollments { get; init; }
}
