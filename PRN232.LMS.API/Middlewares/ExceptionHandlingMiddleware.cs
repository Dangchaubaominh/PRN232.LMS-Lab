using PRN232.LMS.API.ResponseModels;
using PRN232.LMS.Services.Exceptions;

namespace PRN232.LMS.API.Middlewares;

/// <summary>Turns exceptions thrown while handling a request into the standard error envelope.</summary>
public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger, IHostEnvironment environment)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (NotFoundException ex)
        {
            await WriteAsync(context, StatusCodes.Status404NotFound, ApiResponse<object>.Fail(ex.Message));
        }
        catch (BusinessRuleException ex)
        {
            await WriteAsync(context, StatusCodes.Status400BadRequest, ApiResponse<object>.Fail(ex.Message));
        }
        catch (InvalidQueryException ex)
        {
            await WriteAsync(context, StatusCodes.Status400BadRequest, ApiResponse<object>.Fail("Invalid query parameter.", new[] { ex.Message }));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception while processing {Method} {Path}", context.Request.Method, context.Request.Path);
            var detail = environment.IsDevelopment() ? ex.Message : "Internal server error";
            await WriteAsync(context, StatusCodes.Status500InternalServerError, ApiResponse<object>.Fail("An unexpected server error occurred.", new[] { detail }));
        }
    }

    private static Task WriteAsync(HttpContext context, int statusCode, ApiResponse<object> body)
    {
        context.Response.StatusCode = statusCode;
        return context.Response.WriteAsJsonAsync(body);
    }
}
