using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace PRN232.LMS.Repositories.Entities;

public class Student
{
    public int StudentId { get; set; }

    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [MaxLength(100), Unicode(false)]
    public string Email { get; set; } = string.Empty;

    public DateTime DateOfBirth { get; set; }

    public ICollection<Enrollment> Enrollments { get; set; } = [];
}
