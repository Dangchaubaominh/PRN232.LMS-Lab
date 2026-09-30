using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace PRN232.LMS.API.Swagger;

/// <summary>One Swagger document per API version: /swagger/v1/swagger.json, /swagger/v2/swagger.json.</summary>
public class ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider) : IConfigureOptions<SwaggerGenOptions>
{
    public void Configure(SwaggerGenOptions options)
    {
        options.AddSecurityDefinition(AuthorizeOperationFilter.SchemeName, new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description = "Call POST /api/auth/login (e.g. admin / 123456) and paste data.accessToken here, without the 'Bearer ' prefix."
        });
        options.OperationFilter<AuthorizeOperationFilter>();
        options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, $"{typeof(ConfigureSwaggerOptions).Assembly.GetName().Name}.xml"));

        foreach (var description in provider.ApiVersionDescriptions)
        {
            options.SwaggerDoc(description.GroupName, new OpenApiInfo
            {
                Title = "PRN232 LMS API",
                Version = description.ApiVersion.ToString(),
                Description = description.ApiVersion.MajorVersion switch
                {
                    1 => "RESTful LMS API with search, sorting, paging, field selection and relationship expansion. "
                        + "Unversioned /api/... URLs are served by this version.",
                    _ => "Students only. Differences from v1: dateOfBirth is a date (yyyy-MM-dd) and each student "
                        + "carries enrollmentCount instead of an embedded enrollments list. Other resources stay on v1."
                }
            });
        }
    }
}
