using System.ComponentModel.DataAnnotations;

namespace PRN232.LMS.API.RequestModels;

public class RefreshTokenRequest
{
    /// <summary>The refreshToken returned by login or by the previous refresh.</summary>
    [Required, StringLength(200)]
    public string RefreshToken { get; set; } = null!;
}
