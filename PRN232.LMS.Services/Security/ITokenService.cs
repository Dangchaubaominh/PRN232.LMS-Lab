using PRN232.LMS.Services.BusinessModels;

namespace PRN232.LMS.Services.Security;

/// <summary>
/// Issues tokens. Implemented in the API layer (JwtTokenService), which owns the JWT settings and
/// signing key; the service layer only decides when tokens are issued.
/// </summary>
public interface ITokenService
{
    IssuedAccessToken CreateAccessToken(UserModel user);

    /// <summary>A new random refresh token and its expiry.</summary>
    IssuedRefreshToken CreateRefreshToken();
}

public record IssuedAccessToken(string Token, int ExpiresInSeconds);

public record IssuedRefreshToken(string Token, DateTime ExpiresAt);
