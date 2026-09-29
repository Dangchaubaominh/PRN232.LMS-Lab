using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using PRN232.LMS.API.Mappings;
using PRN232.LMS.API.Middlewares;
using PRN232.LMS.API.RequestModels;
using PRN232.LMS.API.ResponseModels;
using PRN232.LMS.API.Shaping;
using PRN232.LMS.Services.Services;

namespace PRN232.LMS.API.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/students")]
public class StudentsController(IStudentService studentService, ILogger<StudentsController> logger) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<CollectionResponse<object>>> GetAll([FromQuery] ListQueryRequest query)
    {
        var fields = FieldSelection<StudentResponse>.Parse(query.Fields);
        var students = await studentService.GetAllAsync(query.ToListQuery());
        return Ok(students.ToCollectionResponse(x => x.ToResponse(), fields, "Students retrieved successfully."));
    }

    [HttpGet("{id:int}", Name = "GetStudentById")]
    public async Task<ActionResult<ApiResponse<StudentResponse>>> GetById([FromRoute] int id)
    {
        var student = await studentService.GetByIdAsync(id);
        return Ok(ApiResponse<StudentResponse>.Ok(student.ToResponse()));
    }

    /// <param name="request">The new student.</param>
    /// <param name="requestId">Optional correlation id; recorded in the audit log line. Generated when omitted.</param>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<StudentResponse>>> Create(
        [FromBody] StudentRequest request,
        [FromHeader(Name = RequestLoggingMiddleware.RequestIdHeader)] string? requestId)
    {
        var student = await studentService.CreateAsync(request.ToModel());
        logger.LogInformation("Student {StudentId} ({StudentCode}) created [{RequestId}]", student.StudentId, student.StudentCode, requestId);
        return CreatedAtRoute("GetStudentById", new { id = student.StudentId },
            ApiResponse<StudentResponse>.Ok(student.ToResponse(), "Student created successfully."));
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
