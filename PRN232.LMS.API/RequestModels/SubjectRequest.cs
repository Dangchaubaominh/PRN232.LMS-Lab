using System.ComponentModel.DataAnnotations;

namespace PRN232.LMS.API.RequestModels;

public class SubjectRequest
{
    [Required]
    [RegularExpression("^[A-Za-z]{3}[0-9]{3}[A-Za-z]?$", ErrorMessage = "SubjectCode must be 3 letters and 3 digits, e.g. PRN232.")]
    public string SubjectCode { get; set; } = null!;

    [Required, StringLength(100)]
    public string SubjectName { get; set; } = null!;

    [Range(1, 10)]
    public int Credit { get; set; }
}
