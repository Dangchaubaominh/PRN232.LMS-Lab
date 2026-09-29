using System.ComponentModel.DataAnnotations;

namespace PRN232.LMS.API.RequestModels;

public class SubjectRequest
{
    [Required, MaxLength(20)]
    public string SubjectCode { get; set; } = null!;

    [Required, MaxLength(100)]
    public string SubjectName { get; set; } = null!;

    [Range(1, 10)]
    public int Credit { get; set; }
}
