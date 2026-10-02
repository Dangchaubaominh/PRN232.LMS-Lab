using System.Globalization;
using System.Threading.RateLimiting;
using PRN232.LMS.API.ResponseModels;

namespace PRN232.LMS.API.Extensions;

public static class RateLimitingExtensions
{
    /// <summary>Policy name for [EnableRateLimiting] on the login endpoint.</summary>
    public const string LoginPolicy = "login";

    /// <summary>
    /// Slows down password guessing: each client IP gets RateLimiting:LoginPermitLimit login attempts
    /// (default 10) per RateLimiting:LoginWindowSeconds (default 60); further attempts get 429.
    /// </summary>
    public static IServiceCollection AddLoginRateLimiting(this IServiceCollection services, IConfiguration configuration)
    {
        var permitLimit = configuration.GetValue("RateLimiting:LoginPermitLimit", 10);
        var window = TimeSpan.FromSeconds(configuration.GetValue("RateLimiting:LoginWindowSeconds", 60));

        return services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.AddPolicy(LoginPolicy, context => RateLimitPartition.GetFixedWindowLimiter(
                context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                _ => new FixedWindowRateLimiterOptions { PermitLimit = permitLimit, Window = window, QueueLimit = 0 }));
            options.OnRejected = async (context, _) =>
            {
                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                {
                    context.HttpContext.Response.Headers.RetryAfter =
                        ((int)Math.Ceiling(retryAfter.TotalSeconds)).ToString(CultureInfo.InvariantCulture);
                }
                await context.HttpContext.WriteApiResponseAsync(
                    StatusCodes.Status429TooManyRequests, ApiResponse<object>.Fail("Too many login attempts. Try again later."));
            };
        });
    }
}
