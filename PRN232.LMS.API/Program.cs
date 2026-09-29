using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PRN232.LMS.API.Extensions;
using PRN232.LMS.API.Middlewares;
using PRN232.LMS.API.ResponseModels;
using PRN232.LMS.Repositories.Data;
using PRN232.LMS.Repositories.Repositories;
using PRN232.LMS.Services.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddSimpleConsole(options =>
{
    options.SingleLine = true;
    options.TimestampFormat = "yyyy-MM-dd HH:mm:ss ";
});

builder.Services
    .AddControllers()
    .ConfigureApiBehaviorOptions(options => options.InvalidModelStateResponseFactory = context =>
        new BadRequestObjectResult(ApiResponse<object>.Fail("Validation failed.", context.ModelState.ToErrorDictionary())));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options => options.SwaggerDoc("v1", new()
{
    Title = "PRN232 LMS API",
    Version = "v1",
    Description = "RESTful LMS API with search, sorting, paging, field selection and relationship expansion."
}));

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection is required.");
builder.Services.AddDbContext<LmsDbContext>(options => options.UseSqlServer(connectionString, sql => sql.EnableRetryOnFailure()));

builder.Services.AddScoped<ILmsRepository, LmsRepository>();
builder.Services.AddScoped<ISemesterService, SemesterService>();
builder.Services.AddScoped<ISubjectService, SubjectService>();
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();

var app = builder.Build();

// Order matters: logging wraps exception handling so it records the final status code.
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();
app.MapGet("/health", async (LmsDbContext db) => await db.Database.CanConnectAsync()
    ? Results.Ok(ApiResponse<object>.Ok(new { status = "healthy" }, "Database is reachable and seeding is complete."))
    : Results.Json(ApiResponse<object>.Fail("Database is unavailable.", new[] { "Database connection failed" }), statusCode: 503));

using (var scope = app.Services.CreateScope())
{
    await DatabaseSeeder.SeedAsync(scope.ServiceProvider.GetRequiredService<LmsDbContext>());
}

app.Run();
