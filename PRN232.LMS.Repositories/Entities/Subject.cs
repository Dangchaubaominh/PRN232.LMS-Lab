using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace PRN232.LMS.Repositories.Entities;

public class Subject
{
    public int SubjectId { get; set; }

    [MaxLength(20), Unicode(false)]
    public string SubjectCode { get; set; } = string.Empty;

    [MaxLength(100)]
    public string SubjectName { get; set; } = string.Empty;

    public int Credit { get; set; }

    public ICollection<Course> Courses { get; set; } = [];
}
