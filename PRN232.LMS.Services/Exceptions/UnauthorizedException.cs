namespace PRN232.LMS.Services.Exceptions;

/// <summary>Wrong credentials or an unusable refresh token. Returned as 401.</summary>
public class UnauthorizedException(string message) : Exception(message);
