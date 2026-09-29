using System.ComponentModel.DataAnnotations;

namespace PRN232.LMS.Repositories.Entities;

public class Semester
{
    public int SemesterId { get; set; }

    [MaxLength(100)]
    public string SemesterName { get; set; } = string.Empty;

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    public ICollection<Course> Courses { get; set; } = [];
}
