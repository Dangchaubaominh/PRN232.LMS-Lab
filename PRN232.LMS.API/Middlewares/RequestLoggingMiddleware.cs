using System.Diagnostics;
using System.Text.RegularExpressions;

namespace PRN232.LMS.API.Middlewares;

/// <summary>
/// Gives every request an id, echoes it in the X-Request-Id response header and logs method, path,
/// status code and execution time. The client's X-Request-Id is reused when it is well-formed;
/// otherwise a new id is generated. Must be registered before <see cref="ExceptionHandlingMiddleware"/>
/// so it logs the status code that middleware writes.
/// </summary>
public partial class RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
{
    public const string RequestIdHeader = "X-Request-Id";

    public async Task InvokeAsync(HttpContext context)
    {
        var requestId = ResolveRequestId(context.Request.Headers[RequestIdHeader]);
        context.TraceIdentifier = requestId;
        // Also written back to the request so [FromHeader(Name = "X-Request-Id")] always binds.
        context.Request.Headers[RequestIdHeader] = requestId;
        context.Response.Headers[RequestIdHeader] = requestId;

        var stopwatch = Stopwatch.StartNew();
        var failed = false;
        try
        {
            await next(context);
        }
        catch
        {
            failed = true;
            throw;
        }
        finally
        {
            stopwatch.Stop();
            var statusCode = failed ? StatusCodes.Status500InternalServerError : context.Response.StatusCode;
            logger.Log(
                LevelFor(statusCode),
                "{Method} {Path}{Query} responded {StatusCode} in {ElapsedMs:0.0} ms [{RequestId}]",
                context.Request.Method,
                context.Request.Path,
                context.Request.QueryString,
                statusCode,
                stopwatch.Elapsed.TotalMilliseconds,
                requestId);
        }
    }

    /// <summary>Only short ids of safe characters are trusted, so a client cannot inject text into the logs.</summary>
    private static string ResolveRequestId(string? header) =>
        header is not null && SafeRequestId().IsMatch(header) ? header : Guid.NewGuid().ToString("N");

    private static LogLevel LevelFor(int statusCode) => statusCode switch
    {
        >= 500 => LogLevel.Error,
        >= 400 => LogLevel.Warning,
        _ => LogLevel.Information
    };

    [GeneratedRegex("^[A-Za-z0-9._:-]{1,64}$")]
    private static partial Regex SafeRequestId();
}
