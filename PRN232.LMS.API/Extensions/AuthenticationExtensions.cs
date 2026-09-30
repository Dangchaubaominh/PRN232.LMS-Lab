using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using PRN232.LMS.API.ResponseModels;
using PRN232.LMS.API.Security;
using PRN232.LMS.Services.Security;

namespace PRN232.LMS.API.Extensions;

public static class AuthenticationExtensions
{
    /// <summary>JWT bearer authentication, validated with the same settings <see cref="JwtTokenService"/> signs with.</summary>
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .ValidateDataAnnotations()
            .Validate(
                options => Encoding.UTF8.GetByteCount(options.Secret) >= JwtOptions.MinimumSecretBytes,
                $"Jwt:Secret must be at least {JwtOptions.MinimumSecretBytes} bytes. Set it with the Jwt__Secret environment variable.")
            .ValidateOnStart();

        services.AddSingleton<ITokenService, JwtTokenService>();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<JwtOptions>>((bearer, jwtOptions) =>
            {
                var jwt = jwtOptions.Value;
                // Keep the claim names as issued ("sub", "role") instead of mapping them to long URIs.
                bearer.MapInboundClaims = false;
                bearer.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwt.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwt.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = JwtTokenService.CreateSigningKey(jwt.Secret),
                    ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(30),
                    NameClaimType = JwtRegisteredClaimNames.UniqueName,
                    RoleClaimType = JwtTokenService.RoleClaim
                };
                bearer.Events = new JwtBearerEvents
                {
                    // 401 and 403 would otherwise have an empty body; send the standard envelope.
                    OnChallenge = async context =>
                    {
                        context.HandleResponse();
                        var expired = context.AuthenticateFailure is SecurityTokenExpiredException;
                        var message = expired ? "Access token has expired."
                            : context.AuthenticateFailure is not null ? "Access token is invalid."
                            : "Authentication required. Send 'Authorization: Bearer <accessToken>'.";
                        context.Response.Headers.WWWAuthenticate = expired
                            ? "Bearer error=\"invalid_token\", error_description=\"The access token has expired\""
                            : "Bearer";
                        await context.HttpContext.WriteApiResponseAsync(StatusCodes.Status401Unauthorized, ApiResponse<object>.Fail(message));
                    },
                    OnForbidden = context => context.HttpContext.WriteApiResponseAsync(
                        StatusCodes.Status403Forbidden, ApiResponse<object>.Fail("You do not have permission to perform this action."))
                };
            });

        services.AddAuthorization();
        return services;
    }
}
