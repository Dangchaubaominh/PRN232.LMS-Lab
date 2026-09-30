using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using PRN232.LMS.API.ResponseModels;

namespace PRN232.LMS.API.Extensions;

public static class HttpContextExtensions
{
    /// <summary>
    /// Writes the envelope from outside MVC (middleware, authentication events) through MVC's
    /// ObjectResult executor, so it is content-negotiated (JSON or XML) like a controller result.
    /// </summary>
    public static async Task WriteApiResponseAsync(this HttpContext context, int statusCode, ApiResponse<object> body)
    {
        var executor = context.RequestServices.GetRequiredService<IActionResultExecutor<ObjectResult>>();
        var actionContext = new ActionContext(context, context.GetRouteData(), new ActionDescriptor());
        await executor.ExecuteAsync(actionContext, new ObjectResult(body) { StatusCode = statusCode });

        // When the Accept header names no format we produce, negotiation answers 406. For an error
        // that would hide the real problem (e.g. a 401 would become a 406), so send it as JSON.
        if (context.Response.StatusCode == StatusCodes.Status406NotAcceptable && !context.Response.HasStarted)
        {
            context.Response.StatusCode = statusCode;
            await context.Response.WriteAsJsonAsync(body);
        }
    }
}
