using Microsoft.AspNetCore.Mvc;
using PRN232.LMS.API.Models;
using PRN232.LMS.API.RequestModels;
using PRN232.LMS.Services.Models;
using PRN232.LMS.Services.Services;
namespace PRN232.LMS.API.Controllers;

[ApiController]
[Route("api/semesters")]
public class SemestersController(ILmsService service) : ControllerBase
{
    [HttpGet] public async Task<IActionResult> GetAll([FromQuery] ListQueryRequest q) => Ok(CollectionResponse<object>.From(await service.GetSemestersAsync(q.ToServiceQuery()), "Semesters retrieved successfully."));
    [HttpGet("{id:int}")] public async Task<IActionResult> Get(int id) { var x = await service.GetSemesterAsync(id); return x is null ? NotFound(ApiResponse<object>.Fail("Semester not found.")) : Ok(ApiResponse<SemesterResponse>.Ok(x)); }
    [HttpPost] public async Task<IActionResult> Create(SemesterRequest r) { var (x, e) = await service.CreateSemesterAsync(r); return e is not null ? BadRequest(ApiResponse<object>.Fail(e)) : CreatedAtAction(nameof(Get), new { id = x!.SemesterId }, ApiResponse<SemesterResponse>.Ok(x, "Semester created successfully.")); }
    [HttpPut("{id:int}")] public async Task<IActionResult> Update(int id, SemesterRequest r) { var (f, e) = await service.UpdateSemesterAsync(id, r); if (!f) return NotFound(ApiResponse<object>.Fail("Semester not found.")); return e is null ? Ok(ApiResponse<object>.Ok(new { id }, "Semester updated successfully.")) : BadRequest(ApiResponse<object>.Fail(e)); }
    [HttpDelete("{id:int}")] public async Task<IActionResult> Delete(int id) => await service.DeleteSemesterAsync(id) ? Ok(ApiResponse<object>.Ok(new { id }, "Semester deleted successfully.")) : NotFound(ApiResponse<object>.Fail("Semester not found."));
}
