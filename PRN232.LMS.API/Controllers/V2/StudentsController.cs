using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PRN232.LMS.API.Mappings;
using PRN232.LMS.API.Middlewares;
using PRN232.LMS.API.RequestModels;
using PRN232.LMS.API.ResponseModels;
using PRN232.LMS.API.Shaping;
using PRN232.LMS.Services.Security;
using PRN232.LMS.Services.Services;

namespace PRN232.LMS.API.Controllers.V2;

/// <summary>
/// Students, API v2: same operations and request body as v1, but responses use
/// <see cref="StudentResponseV2"/> (date-only dateOfBirth, enrollmentCount).
/// </summary>
[ApiController]
[Authorize]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/students")]
public class StudentsController(IStudentService studentService, ILogger<StudentsController> logger) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<CollectionResponse<object>>> GetAll([FromQuery] StudentQueryRequest query)
    {
        var fields = FieldSelection<StudentResponseV2>.Parse(query.Fields);
        var students = await studentService.GetAllAsync(query.ToListQuery(), includeEnrollmentCount: true);
        return Ok(students.ToCollectionResponse(x => x.ToResponseV2(), fields, "Students retrieved successfully."));
    }

    [HttpGet("{id:int}", Name = "GetStudentByIdV2")]
    public async Task<ActionResult<ApiResponse<StudentResponseV2>>> GetById([FromRoute] int id)
    {
        var student = await studentService.GetByIdAsync(id);
        return Ok(ApiResponse<StudentResponseV2>.Ok(student.ToResponseV2()));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<StudentResponseV2>>> Create(
        [FromBody] StudentRequest request,
        [FromHeader(Name = RequestLoggingMiddleware.RequestIdHeader)] string? requestId)
    {
        var student = await studentService.CreateAsync(request.ToModel());
        logger.LogInformation("Student {StudentId} ({StudentCode}) created [{RequestId}]", student.StudentId, student.StudentCode, requestId);
        return CreatedAtRoute("GetStudentByIdV2", new { id = student.StudentId },
            ApiResponse<StudentResponseV2>.Ok(student.ToResponseV2(), "Student created successfully."));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse<object>>> Update(
        [FromRoute] int id,
        [FromBody] StudentRequest request,
        [FromHeader(Name = RequestLoggingMiddleware.RequestIdHeader)] string? requestId)
    {
        await studentService.UpdateAsync(id, request.ToModel());
        logger.LogInformation("Student {StudentId} updated [{RequestId}]", id, requestId);
        return Ok(ApiResponse<object>.Ok(new { id }, "Student updated successfully."));
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(
        [FromRoute] int id,
        [FromHeader(Name = RequestLoggingMiddleware.RequestIdHeader)] string? requestId)
    {
        await studentService.DeleteAsync(id);
        logger.LogInformation("Student {StudentId} deleted [{RequestId}]", id, requestId);
        return Ok(ApiResponse<object>.Ok(new { id }, "Student deleted successfully."));
    }
}
