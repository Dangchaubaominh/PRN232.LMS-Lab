using System.ComponentModel.DataAnnotations;

namespace PRN232.LMS.API.RequestModels;

public class CourseRequest
{
    [Required, MaxLength(100)]
    public string CourseName { get; set; } = null!;

    [Range(1, int.MaxValue)]
    public int SemesterId { get; set; }

    [Range(1, int.MaxValue)]
    public int SubjectId { get; set; }
}
