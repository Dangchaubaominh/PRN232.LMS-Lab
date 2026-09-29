using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace PRN232.LMS.Repositories.Entities;

public class Semester
{
    public int SemesterId { get; set; }
    [MaxLength(100)] public string SemesterName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public ICollection<Course> Courses { get; set; } = [];
}

public class Subject
{
    public int SubjectId { get; set; }
    [MaxLength(20), Unicode(false)] public string SubjectCode { get; set; } = string.Empty;
    [MaxLength(100)] public string SubjectName { get; set; } = string.Empty;
    public int Credit { get; set; }
    public ICollection<Course> Courses { get; set; } = [];
}

public class Course
{
    public int CourseId { get; set; }
    [MaxLength(100)] public string CourseName { get; set; } = string.Empty;
    public int SemesterId { get; set; }
    public int SubjectId { get; set; }
    public Semester Semester { get; set; } = null!;
    public Subject Subject { get; set; } = null!;
    public ICollection<Enrollment> Enrollments { get; set; } = [];
}

public class Student
{
    public int StudentId { get; set; }
    [MaxLength(100)] public string FullName { get; set; } = string.Empty;
    [MaxLength(100), Unicode(false)] public string Email { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public ICollection<Enrollment> Enrollments { get; set; } = [];
}

public class Enrollment
{
    public int EnrollmentId { get; set; }
    public int StudentId { get; set; }
    public int CourseId { get; set; }
    public DateTime EnrollDate { get; set; }
    [MaxLength(20), Unicode(false)] public string Status { get; set; } = string.Empty;
    public Student Student { get; set; } = null!;
    public Course Course { get; set; } = null!;
}
