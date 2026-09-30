using PRN232.LMS.Services.BusinessModels;

namespace PRN232.LMS.Services.Services;

public interface IAuthService
{
    /// <exception cref="Exceptions.UnauthorizedException">Unknown user or wrong password.</exception>
    Task<AuthResult> LoginAsync(string username, string password);

    /// <summary>Exchanges a refresh token for a new token pair; the old refresh token stops working.</summary>
    /// <exception cref="Exceptions.UnauthorizedException">Unknown, revoked or expired refresh token.</exception>
    Task<AuthResult> RefreshAsync(string refreshToken);

    /// <summary>Revokes the refresh token. Unknown or already revoked tokens are ignored.</summary>
    Task LogoutAsync(string refreshToken);
}
