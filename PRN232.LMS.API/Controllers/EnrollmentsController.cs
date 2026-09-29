using Microsoft.AspNetCore.Mvc;
using PRN232.LMS.API.Mappings;
using PRN232.LMS.API.RequestModels;
using PRN232.LMS.API.ResponseModels;
using PRN232.LMS.API.Shaping;
using PRN232.LMS.Services.Services;

namespace PRN232.LMS.API.Controllers;

[ApiController]
[Route("api/enrollments")]
public class EnrollmentsController(IEnrollmentService enrollmentService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<CollectionResponse<object>>> GetAll([FromQuery] ListQueryRequest query)
    {
        var fields = FieldSelection<EnrollmentResponse>.Parse(query.Fields);
        var enrollments = await enrollmentService.GetAllAsync(query.ToListQuery());
        return Ok(enrollments.ToCollectionResponse(x => x.ToResponse(), fields, "Enrollments retrieved successfully."));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<EnrollmentResponse>>> GetById([FromRoute] int id)
    {
        var enrollment = await enrollmentService.GetByIdAsync(id);
        return Ok(ApiResponse<EnrollmentResponse>.Ok(enrollment.ToResponse()));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<EnrollmentResponse>>> Create([FromBody] EnrollmentRequest request)
    {
        var enrollment = await enrollmentService.CreateAsync(request.ToModel());
        return CreatedAtAction(nameof(GetById), new { id = enrollment.EnrollmentId },
            ApiResponse<EnrollmentResponse>.Ok(enrollment.ToResponse(), "Enrollment created successfully."));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse<object>>> Update([FromRoute] int id, [FromBody] EnrollmentRequest request)
    {
        await enrollmentService.UpdateAsync(id, request.ToModel());
        return Ok(ApiResponse<object>.Ok(new { id }, "Enrollment updated successfully."));
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete([FromRoute] int id)
    {
        await enrollmentService.DeleteAsync(id);
        return Ok(ApiResponse<object>.Ok(new { id }, "Enrollment deleted successfully."));
    }
}
