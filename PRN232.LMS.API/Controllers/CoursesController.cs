using Microsoft.AspNetCore.Mvc;
using PRN232.LMS.API.Models;
using PRN232.LMS.API.RequestModels;
using PRN232.LMS.Services.Models;
using PRN232.LMS.Services.Services;
namespace PRN232.LMS.API.Controllers;

[ApiController]
[Route("api/courses")]
public class CoursesController(ILmsService service) : ControllerBase
{
    [HttpGet] public async Task<IActionResult> GetAll([FromQuery] ListQueryRequest q) => Ok(CollectionResponse<object>.From(await service.GetCoursesAsync(q.ToServiceQuery()), "Courses retrieved successfully."));
    [HttpGet("{id:int}")] public async Task<IActionResult> Get(int id) { var x = await service.GetCourseAsync(id); return x is null ? NotFound(ApiResponse<object>.Fail("Course not found.")) : Ok(ApiResponse<CourseResponse>.Ok(x)); }
    [HttpPost] public async Task<IActionResult> Create(CourseRequest r) { var (x, e) = await service.CreateCourseAsync(r); return e is not null ? BadRequest(ApiResponse<object>.Fail(e)) : CreatedAtAction(nameof(Get), new { id = x!.CourseId }, ApiResponse<CourseResponse>.Ok(x, "Course created successfully.")); }
    [HttpPut("{id:int}")] public async Task<IActionResult> Update(int id, CourseRequest r) { var (f, e) = await service.UpdateCourseAsync(id, r); if (!f) return NotFound(ApiResponse<object>.Fail("Course not found.")); return e is null ? Ok(ApiResponse<object>.Ok(new { id }, "Course updated successfully.")) : BadRequest(ApiResponse<object>.Fail(e)); }
    [HttpDelete("{id:int}")] public async Task<IActionResult> Delete(int id) => await service.DeleteCourseAsync(id) ? Ok(ApiResponse<object>.Ok(new { id }, "Course deleted successfully.")) : NotFound(ApiResponse<object>.Fail("Course not found."));
}
