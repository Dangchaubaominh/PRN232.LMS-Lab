using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PRN232.LMS.API.Models;
using PRN232.LMS.Repositories.Data;
using PRN232.LMS.Repositories.Repositories;
using PRN232.LMS.Services.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers().ConfigureApiBehaviorOptions(options => options.InvalidModelStateResponseFactory = context => new BadRequestObjectResult(ApiResponse<object>.Fail("Validation failed.", context.ModelState.Where(x => x.Value?.Errors.Count > 0).ToDictionary(x => x.Key, x => x.Value!.Errors.Select(e => e.ErrorMessage).ToArray()))));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options => options.SwaggerDoc("v1", new() { Title = "PRN232 LMS API", Version = "v1", Description = "RESTful LMS API with search, sorting, paging, field selection and relationship expansion." }));
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection is required.");
builder.Services.AddDbContext<LmsDbContext>(options => options.UseSqlServer(connectionString, sql => sql.EnableRetryOnFailure()));
builder.Services.AddScoped<ILmsRepository, LmsRepository>();
builder.Services.AddScoped<ILmsService, LmsService>();
var app = builder.Build();
app.UseSwagger(); app.UseSwaggerUI();
app.Use(async (context, next) => { try { await next(); } catch (ArgumentException ex) { context.Response.StatusCode = 400; await context.Response.WriteAsJsonAsync(ApiResponse<object>.Fail("Invalid query parameter.", new[] { ex.Message })); } catch (Exception ex) { context.Response.StatusCode = 500; await context.Response.WriteAsJsonAsync(ApiResponse<object>.Fail("An unexpected server error occurred.", new[] { app.Environment.IsDevelopment() ? ex.Message : "Internal server error" })); } });
app.MapControllers();
using (var scope = app.Services.CreateScope()) await DatabaseSeeder.SeedAsync(scope.ServiceProvider.GetRequiredService<LmsDbContext>());
app.MapGet("/health", async (LmsDbContext db) => await db.Database.CanConnectAsync()
    ? Results.Ok(ApiResponse<object>.Ok(new { status = "healthy" }, "Database is reachable and seeding is complete."))
    : Results.Json(ApiResponse<object>.Fail("Database is unavailable.", new[] { "Database connection failed" }), statusCode: 503));
app.Run();
