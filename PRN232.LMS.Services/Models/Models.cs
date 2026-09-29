using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PRN232.LMS.Services.Models;

// Business models used by service-layer rules.
public record SemesterModel(int SemesterId, string SemesterName, DateTime StartDate, DateTime EndDate);
public record SubjectModel(int SubjectId, string SubjectCode, string SubjectName, int Credit);
public record CourseModel(int CourseId, string CourseName, int SemesterId, int SubjectId);
public record StudentModel(int StudentId, string FullName, string Email, DateTime DateOfBirth);
public record EnrollmentModel(int EnrollmentId, int StudentId, int CourseId, DateTime EnrollDate, string Status);

public record SemesterRequest([Required, MaxLength(100)] string SemesterName, DateTime StartDate, DateTime EndDate);
public record SubjectRequest([Required, MaxLength(20)] string SubjectCode, [Required, MaxLength(100)] string SubjectName, [Range(1, 10)] int Credit);
public record CourseRequest([Required, MaxLength(100)] string CourseName, [Range(1, int.MaxValue)] int SemesterId, [Range(1, int.MaxValue)] int SubjectId);
public record StudentRequest([Required, MaxLength(100)] string FullName, [Required, EmailAddress, MaxLength(100)] string Email, DateTime DateOfBirth);
public record EnrollmentRequest([Range(1, int.MaxValue)] int StudentId, [Range(1, int.MaxValue)] int CourseId, DateTime EnrollDate, [Required, RegularExpression("Active|Completed|Dropped")] string Status);

// A relation is left out of the JSON entirely when the client did not ask for it through `expand`,
// so a response never carries "courses": null noise. The envelope members (success, message, data,
// errors) are deliberately NOT covered by this rule: the API contract requires all four to be
// present on every response, even when their value is null.
public record SemesterResponse(int SemesterId, string SemesterName, DateTime StartDate, DateTime EndDate, [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] object? Courses = null);
public record SubjectResponse(int SubjectId, string SubjectCode, string SubjectName, int Credit, [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] object? Courses = null);
public record CourseResponse(int CourseId, string CourseName, int SemesterId, int SubjectId, [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] object? Semester = null, [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] object? Subject = null, [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] object? Enrollments = null);
public record StudentResponse(int StudentId, string FullName, string Email, DateTime DateOfBirth, [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] object? Enrollments = null);
public record EnrollmentResponse(int EnrollmentId, int StudentId, int CourseId, DateTime EnrollDate, string Status, [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] object? Student = null, [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] object? Course = null);

public class ListQuery
{
    public string? Search { get; set; }
    public string? Status { get; set; }
    public int? SemesterId { get; set; }
    public int? SubjectId { get; set; }
    public int? StudentId { get; set; }
    public int? CourseId { get; set; }
    public string? Sort { get; set; }
    [Range(1, int.MaxValue)] public int Page { get; set; } = 1;
    [Range(1, 100)] public int Size { get; set; } = 10;
    public string? Fields { get; set; }
    public string? Expand { get; set; }
}

public record Pagination(int Page, int PageSize, int TotalItems, int TotalPages);
public record PagedResult(IReadOnlyList<object> Items, Pagination Pagination);
public record ServiceResult<T>(bool Success, string Message, T? Data = default, IReadOnlyDictionary<string, string[]>? Errors = null)
{
    public static ServiceResult<T> Ok(T data, string message = "Request processed successfully") => new(true, message, data);
    public static ServiceResult<T> Fail(string message, IReadOnlyDictionary<string, string[]>? errors = null) => new(false, message, default, errors);
}
