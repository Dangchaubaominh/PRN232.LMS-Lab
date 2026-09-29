using System.ComponentModel.DataAnnotations;

namespace PRN232.LMS.API.RequestModels;

public record CreateStudentRequest([Required, MaxLength(100)] string FullName, [Required, EmailAddress, MaxLength(100)] string Email, DateTime DateOfBirth);
public class StudentQueryRequest
{
    public string? Search { get; set; }
    public string? Sort { get; set; }
    [Range(1, int.MaxValue)] public int Page { get; set; } = 1;
    [Range(1, 100)] public int Size { get; set; } = 10;
    public string? Fields { get; set; }
    public string? Expand { get; set; }
}
