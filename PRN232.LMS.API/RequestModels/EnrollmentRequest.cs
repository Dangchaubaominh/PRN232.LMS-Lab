using System.ComponentModel.DataAnnotations;

namespace PRN232.LMS.API.RequestModels;

public class EnrollmentRequest
{
    [Range(1, int.MaxValue)]
    public int StudentId { get; set; }

    [Range(1, int.MaxValue)]
    public int CourseId { get; set; }

    public DateTime EnrollDate { get; set; }

    [Required, RegularExpression("Active|Completed|Dropped")]
    public string Status { get; set; } = null!;
}
