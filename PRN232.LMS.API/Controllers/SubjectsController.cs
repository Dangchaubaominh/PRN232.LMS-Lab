using Microsoft.AspNetCore.Mvc;
using PRN232.LMS.API.Models;
using PRN232.LMS.API.RequestModels;
using PRN232.LMS.Services.Models;
using PRN232.LMS.Services.Services;
namespace PRN232.LMS.API.Controllers;

[ApiController]
[Route("api/subjects")]
public class SubjectsController(ILmsService service) : ControllerBase
{
    [HttpGet] public async Task<IActionResult> GetAll([FromQuery] ListQueryRequest q) => Ok(CollectionResponse<object>.From(await service.GetSubjectsAsync(q.ToServiceQuery()), "Subjects retrieved successfully."));
    [HttpGet("{id:int}")] public async Task<IActionResult> Get(int id) { var x = await service.GetSubjectAsync(id); return x is null ? NotFound(ApiResponse<object>.Fail("Subject not found.")) : Ok(ApiResponse<SubjectResponse>.Ok(x)); }
    [HttpPost] public async Task<IActionResult> Create(SubjectRequest r) { var (x, e) = await service.CreateSubjectAsync(r); return e is not null ? BadRequest(ApiResponse<object>.Fail(e)) : CreatedAtAction(nameof(Get), new { id = x!.SubjectId }, ApiResponse<SubjectResponse>.Ok(x, "Subject created successfully.")); }
    [HttpPut("{id:int}")] public async Task<IActionResult> Update(int id, SubjectRequest r) { var (f, e) = await service.UpdateSubjectAsync(id, r); if (!f) return NotFound(ApiResponse<object>.Fail("Subject not found.")); return e is null ? Ok(ApiResponse<object>.Ok(new { id }, "Subject updated successfully.")) : BadRequest(ApiResponse<object>.Fail(e)); }
    [HttpDelete("{id:int}")] public async Task<IActionResult> Delete(int id) => await service.DeleteSubjectAsync(id) ? Ok(ApiResponse<object>.Ok(new { id }, "Subject deleted successfully.")) : NotFound(ApiResponse<object>.Fail("Subject not found."));
}
