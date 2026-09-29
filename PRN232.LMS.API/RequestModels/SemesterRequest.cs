namespace PRN232.LMS.API.RequestModels;

/// <summary>
/// Validated with FluentValidation (<c>SemesterRequestValidator</c>) instead of attributes, which is
/// why the name is nullable: a non-nullable string would get an implicit [Required] from MVC and be
/// rejected before the validator runs.
/// </summary>
public class SemesterRequest
{
    public string? SemesterName { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}
