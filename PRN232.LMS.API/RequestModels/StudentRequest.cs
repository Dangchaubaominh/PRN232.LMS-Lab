using System.ComponentModel.DataAnnotations;
using PRN232.LMS.API.Validation;

namespace PRN232.LMS.API.RequestModels;

public class StudentRequest
{
    /// <example>SE193293</example>
    [Required, FptuStudentCode]
    public string StudentCode { get; set; } = null!;

    [Required, StringLength(100, MinimumLength = 2)]
    public string FullName { get; set; } = null!;

    [Required, EmailAddress, StringLength(100)]
    public string Email { get; set; } = null!;

    /// <summary>Optional.</summary>
    /// <example>0901234567</example>
    [Phone, StringLength(15, MinimumLength = 10)]
    public string? Phone { get; set; }

    public DateTime DateOfBirth { get; set; }
}
