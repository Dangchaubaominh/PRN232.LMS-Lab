namespace PRN232.LMS.Services.BusinessModels;

/// <summary>Tokens handed to a client after login or refresh.</summary>
public record AuthResult(string AccessToken, string RefreshToken, int ExpiresIn);
