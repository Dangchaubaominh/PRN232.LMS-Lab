using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace PRN232.LMS.Repositories.Entities;

public class Student
{
    public int StudentId { get; set; }

    /// <summary>FPTU student code, e.g. SE193293. Unique.</summary>
    [MaxLength(8), Unicode(false)]
    public string StudentCode { get; set; } = string.Empty;

    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [MaxLength(100), Unicode(false)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(15), Unicode(false)]
    public string? Phone { get; set; }

    public DateTime DateOfBirth { get; set; }

    public ICollection<Enrollment> Enrollments { get; set; } = [];
}
