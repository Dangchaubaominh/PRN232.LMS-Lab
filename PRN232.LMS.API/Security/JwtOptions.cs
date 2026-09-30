using System.ComponentModel.DataAnnotations;

namespace PRN232.LMS.API.Security;

/// <summary>
/// The "Jwt" configuration section. Secret has no value in appsettings: it must come from the
/// Jwt__Secret environment variable (docker-compose.yml, launchSettings.json).
/// </summary>
public class JwtOptions
{
    public const string SectionName = "Jwt";

    /// <summary>HMAC-SHA256 key; HS256 needs at least 256 bits, i.e. 32 bytes.</summary>
    public const int MinimumSecretBytes = 32;

    [Required]
    public string Issuer { get; set; } = string.Empty;

    [Required]
    public string Audience { get; set; } = string.Empty;

    public string Secret { get; set; } = string.Empty;

    [Range(1, 1440)]
    public int AccessTokenMinutes { get; set; } = 60;

    [Range(1, 90)]
    public int RefreshTokenDays { get; set; } = 7;
}
