using Microsoft.AspNetCore.Mvc;
using PRN232.LMS.API.Models;
using PRN232.LMS.API.RequestModels;
using PRN232.LMS.Services.Models;
using PRN232.LMS.Services.Services;
namespace PRN232.LMS.API.Controllers;

[ApiController]
[Route("api/enrollments")]
public class EnrollmentsController(ILmsService service) : ControllerBase
{
    [HttpGet] public async Task<IActionResult> GetAll([FromQuery] ListQueryRequest query) => Ok(CollectionResponse<object>.From(await service.GetEnrollmentsAsync(query.ToServiceQuery()), "Enrollments retrieved successfully."));
    [HttpGet("{id:int}")] public async Task<IActionResult> Get(int id) { var x = await service.GetEnrollmentAsync(id); return x is null ? NotFound(ApiResponse<object>.Fail("Enrollment not found.")) : Ok(ApiResponse<EnrollmentResponse>.Ok(x)); }
    [HttpPost] public async Task<IActionResult> Create(EnrollmentRequest request) { var (x, error) = await service.CreateEnrollmentAsync(request); return error is not null ? BadRequest(ApiResponse<object>.Fail(error)) : CreatedAtAction(nameof(Get), new { id = x!.EnrollmentId }, ApiResponse<EnrollmentResponse>.Ok(x, "Enrollment created successfully.")); }
    [HttpPut("{id:int}")] public async Task<IActionResult> Update(int id, EnrollmentRequest request) { var (found, error) = await service.UpdateEnrollmentAsync(id, request); if (!found) return NotFound(ApiResponse<object>.Fail("Enrollment not found.")); return error is null ? Ok(ApiResponse<object>.Ok(new { id }, "Enrollment updated successfully.")) : BadRequest(ApiResponse<object>.Fail(error)); }
    [HttpDelete("{id:int}")] public async Task<IActionResult> Delete(int id) => await service.DeleteEnrollmentAsync(id) ? Ok(ApiResponse<object>.Ok(new { id }, "Enrollment deleted successfully.")) : NotFound(ApiResponse<object>.Fail("Enrollment not found."));
}
