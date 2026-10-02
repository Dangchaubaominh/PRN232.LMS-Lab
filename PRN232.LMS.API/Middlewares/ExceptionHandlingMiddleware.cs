using PRN232.LMS.API.Extensions;
using PRN232.LMS.API.ResponseModels;
using PRN232.LMS.Repositories.Exceptions;
using PRN232.LMS.Services.Exceptions;

namespace PRN232.LMS.API.Middlewares;

/// <summary>
/// Turns exceptions thrown while handling a request into the standard error envelope. Unexpected
/// exceptions are logged in full but reach the client only as a generic 500, whatever the environment.
/// The envelope is content-negotiated (JSON or XML) like any controller result.
/// </summary>
public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception) when (!context.Response.HasStarted)
        {
            var (statusCode, body) = ToErrorResponse(exception);
            if (statusCode == StatusCodes.Status500InternalServerError)
            {
                logger.LogError(exception, "Unhandled exception while processing {Method} {Path} [{RequestId}]",
                    context.Request.Method, context.Request.Path, context.TraceIdentifier);
            }

            if (statusCode == StatusCodes.Status401Unauthorized)
            {
                context.Response.Headers.WWWAuthenticate = "Bearer";
            }
            await context.WriteApiResponseAsync(statusCode, body);
        }
    }

    private static (int StatusCode, ApiResponse<object> Body) ToErrorResponse(Exception exception) => exception switch
    {
        UnauthorizedException ex => (StatusCodes.Status401Unauthorized, ApiResponse<object>.Fail(ex.Message)),
        NotFoundException ex => (StatusCodes.Status404NotFound, ApiResponse<object>.Fail(ex.Message)),
        ConflictException ex => (StatusCodes.Status409Conflict, ApiResponse<object>.Fail(ex.Message)),
        DataConflictException ex => (StatusCodes.Status409Conflict, ApiResponse<object>.Fail(ex.Message)),
        BusinessRuleException ex => (StatusCodes.Status400BadRequest, ApiResponse<object>.Fail(ex.Message)),
        InvalidQueryException ex => (StatusCodes.Status400BadRequest, ApiResponse<object>.Fail("Invalid query parameter.", new[] { ex.Message })),
        // Never echo the message of an unexpected exception: it can reveal SQL, table or server details.
        _ => (StatusCodes.Status500InternalServerError, ApiResponse<object>.Fail("Internal server error"))
    };
}
