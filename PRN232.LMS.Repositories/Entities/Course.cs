using System.ComponentModel.DataAnnotations;

namespace PRN232.LMS.Repositories.Entities;

public class Course
{
    public int CourseId { get; set; }

    [MaxLength(100)]
    public string CourseName { get; set; } = string.Empty;

    public int SemesterId { get; set; }
    public int SubjectId { get; set; }

    public Semester Semester { get; set; } = null!;
    public Subject Subject { get; set; } = null!;
    public ICollection<Enrollment> Enrollments { get; set; } = [];
}
