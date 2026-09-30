namespace PRN232.LMS.API.ResponseModels;

public class AuthResponse
{
    /// <summary>JWT to send as "Authorization: Bearer ...".</summary>
    public string AccessToken { get; init; } = string.Empty;

    /// <summary>Single-use token for POST /api/auth/refresh-token.</summary>
    public string RefreshToken { get; init; } = string.Empty;

    /// <summary>Lifetime of the access token in seconds.</summary>
    public int ExpiresIn { get; init; }
}
