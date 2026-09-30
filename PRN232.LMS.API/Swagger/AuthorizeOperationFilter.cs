using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace PRN232.LMS.API.Swagger;

/// <summary>
/// Marks operations that need a token with the "Bearer" scheme (padlock icon, token sent after
/// clicking Authorize) and documents their 401 / 403 responses. [AllowAnonymous] operations stay open.
/// </summary>
public class AuthorizeOperationFilter : IOperationFilter
{
    public const string SchemeName = "Bearer";

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var metadata = context.ApiDescription.ActionDescriptor.EndpointMetadata;
        if (metadata.OfType<IAllowAnonymous>().Any() || !metadata.OfType<IAuthorizeData>().Any())
        {
            return;
        }

        operation.Security.Add(new OpenApiSecurityRequirement
        {
            [new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = SchemeName } }] = []
        });
        operation.Responses.TryAdd("401", new OpenApiResponse { Description = "Missing, invalid or expired access token" });

        var roles = metadata.OfType<IAuthorizeData>().Select(a => a.Roles).Where(r => !string.IsNullOrEmpty(r)).ToList();
        if (roles.Count > 0)
        {
            operation.Responses.TryAdd("403", new OpenApiResponse { Description = $"Requires role: {string.Join(", ", roles)}" });
        }
    }
}
