using System.ComponentModel.DataAnnotations;

namespace PRN232.LMS.API.RequestModels;

public class SemesterRequest
{
    [Required, MaxLength(100)]
    public string SemesterName { get; set; } = null!;

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}
