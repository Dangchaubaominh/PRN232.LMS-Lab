namespace PRN232.LMS.API.ResponseModels;

/// <summary>The caller as seen in their access token.</summary>
public class CurrentUserResponse
{
    public int UserId { get; init; }
    public string Username { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
}
