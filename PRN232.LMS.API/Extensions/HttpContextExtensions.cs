using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using PRN232.LMS.API.ResponseModels;

namespace PRN232.LMS.API.Extensions;

public static class HttpContextExtensions
{
    /// <summary>
    /// Writes the envelope from outside MVC (middleware) through MVC's ObjectResult executor, so it
    /// is content-negotiated (JSON / XML / 406) exactly like a controller result.
    /// </summary>
    public static Task WriteApiResponseAsync(this HttpContext context, int statusCode, ApiResponse<object> body)
    {
        var executor = context.RequestServices.GetRequiredService<IActionResultExecutor<ObjectResult>>();
        var actionContext = new ActionContext(context, context.GetRouteData(), new ActionDescriptor());
        return executor.ExecuteAsync(actionContext, new ObjectResult(body) { StatusCode = statusCode });
    }
}
