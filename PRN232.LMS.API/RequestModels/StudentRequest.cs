using System.ComponentModel.DataAnnotations;

namespace PRN232.LMS.API.RequestModels;

public class StudentRequest
{
    [Required, MaxLength(100)]
    public string FullName { get; set; } = null!;

    [Required, EmailAddress, MaxLength(100)]
    public string Email { get; set; } = null!;

    public DateTime DateOfBirth { get; set; }
}
