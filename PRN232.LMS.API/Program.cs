using Asp.Versioning;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PRN232.LMS.API.Extensions;
using PRN232.LMS.API.Filters;
using PRN232.LMS.API.Formatters;
using PRN232.LMS.API.Middlewares;
using PRN232.LMS.API.ResponseModels;
using PRN232.LMS.API.Security;
using PRN232.LMS.API.Swagger;
using PRN232.LMS.API.Validators;
using PRN232.LMS.Repositories.Data;
using PRN232.LMS.Repositories.Repositories;
using PRN232.LMS.Services.Security;
using PRN232.LMS.Services.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddSimpleConsole(options =>
{
    options.SingleLine = true;
    options.TimestampFormat = "yyyy-MM-dd HH:mm:ss ";
});

builder.Services
    .AddControllers(options =>
    {
        // Content negotiation: JSON by default, XML on request, 406 for any other Accept type.
        options.ReturnHttpNotAcceptable = true;
        options.OutputFormatters.Add(new ApiXmlOutputFormatter());
        // Data annotations are checked first by [ApiController]; FluentValidation validators run after them.
        options.Filters.Add<FluentValidationFilter>();
    })
    .ConfigureApiBehaviorOptions(options => options.InvalidModelStateResponseFactory = context =>
        new BadRequestObjectResult(ApiResponse<object>.Fail("Validation failed.", context.ModelState.ToErrorDictionary())));

builder.Services.AddValidatorsFromAssemblyContaining<SemesterRequestValidator>();

// URL segment versioning: /api/v1/..., /api/v2/...; responses list the versions in api-supported-versions.
builder.Services
    .AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(1, 0);
        options.ReportApiVersions = true;
        options.ApiVersionReader = new UrlSegmentApiVersionReader();
    })
    .AddMvc()
    .AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'V";
        options.SubstituteApiVersionInUrl = true;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.ConfigureOptions<ConfigureSwaggerOptions>();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection is required.");
builder.Services.AddDbContext<LmsDbContext>(options => options.UseSqlServer(connectionString, sql => sql.EnableRetryOnFailure()));

builder.Services.AddScoped<ILmsRepository, LmsRepository>();
builder.Services.AddScoped<ISemesterService, SemesterService>();
builder.Services.AddScoped<ISubjectService, SubjectService>();
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();

builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();

var app = builder.Build();

// Fail fast, before touching the database, when the JWT settings (e.g. Jwt__Secret) are missing or too weak.
_ = app.Services.GetRequiredService<IOptions<JwtOptions>>().Value;

// Order matters: logging wraps exception handling so it records the final status code.
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Unmatched URLs (including failed route constraints such as /students/abc) and wrong HTTP methods
// get the standard envelope instead of an empty body.
app.UseStatusCodePages(context => context.HttpContext.Response.StatusCode switch
{
    StatusCodes.Status404NotFound => context.HttpContext.WriteApiResponseAsync(StatusCodes.Status404NotFound, ApiResponse<object>.Fail("Resource not found.")),
    StatusCodes.Status405MethodNotAllowed => context.HttpContext.WriteApiResponseAsync(StatusCodes.Status405MethodNotAllowed, ApiResponse<object>.Fail("Method not allowed.")),
    _ => Task.CompletedTask
});

// Lab 1 clients call unversioned URLs (/api/students); serve them with v1. /api/auth is not versioned.
app.UseRewriter(new RewriteOptions().AddRewrite(@"(?i)^api/(?!v\d+(?:/|$)|auth(?:/|$))(.*)$", "api/v1/$1", skipRemainingRules: true));
// Explicit so that routing sees the rewritten path (the implicit UseRouting runs before all middleware).
app.UseRouting();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    foreach (var description in app.DescribeApiVersions())
    {
        options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", description.GroupName.ToUpperInvariant());
    }
    // Keep the token entered with the Authorize button across page reloads.
    options.EnablePersistAuthorization();
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapGet("/health", async (LmsDbContext db) => await db.Database.CanConnectAsync()
    ? Results.Ok(ApiResponse<object>.Ok(new { status = "healthy" }, "Database is reachable and seeding is complete."))
    : Results.Json(ApiResponse<object>.Fail("Database is unavailable.", new[] { "Database connection failed" }), statusCode: 503));

using (var scope = app.Services.CreateScope())
{
    var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
    await DatabaseSeeder.SeedAsync(scope.ServiceProvider.GetRequiredService<LmsDbContext>(), passwordHasher.Hash);
}

app.Run();
