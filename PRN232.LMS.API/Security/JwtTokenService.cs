using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using PRN232.LMS.Services.BusinessModels;
using PRN232.LMS.Services.Security;

namespace PRN232.LMS.API.Security;

/// <summary>Signs HS256 access tokens and generates random refresh tokens.</summary>
public class JwtTokenService(IOptions<JwtOptions> options) : ITokenService
{
    public const string RoleClaim = "role";

    private readonly JwtOptions _options = options.Value;
    private readonly JsonWebTokenHandler _handler = new();

    public IssuedAccessToken CreateAccessToken(UserModel user)
    {
        var now = DateTime.UtcNow;
        var expires = now.AddMinutes(_options.AccessTokenMinutes);
        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = _options.Issuer,
            Audience = _options.Audience,
            IssuedAt = now,
            NotBefore = now,
            Expires = expires,
            Claims = new Dictionary<string, object>
            {
                [JwtRegisteredClaimNames.Sub] = user.UserId.ToString(),
                [JwtRegisteredClaimNames.UniqueName] = user.Username,
                [RoleClaim] = user.Role,
                [JwtRegisteredClaimNames.Jti] = Guid.NewGuid().ToString()
            },
            SigningCredentials = new SigningCredentials(CreateSigningKey(_options.Secret), SecurityAlgorithms.HmacSha256)
        };

        return new IssuedAccessToken(_handler.CreateToken(descriptor), (int)(expires - now).TotalSeconds);
    }

    public IssuedRefreshToken CreateRefreshToken() =>
        new(Base64UrlEncoder.Encode(RandomNumberGenerator.GetBytes(64)), DateTime.UtcNow.AddDays(_options.RefreshTokenDays));

    public static SymmetricSecurityKey CreateSigningKey(string secret) => new(Encoding.UTF8.GetBytes(secret));
}
