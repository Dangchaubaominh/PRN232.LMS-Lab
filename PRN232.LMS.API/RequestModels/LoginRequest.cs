using System.ComponentModel.DataAnnotations;

namespace PRN232.LMS.API.RequestModels;

public class LoginRequest
{
    /// <example>admin</example>
    [Required, StringLength(50)]
    public string Username { get; set; } = null!;

    /// <example>123456</example>
    [Required, StringLength(100)]
    public string Password { get; set; } = null!;
}
