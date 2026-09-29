using Microsoft.AspNetCore.Mvc;
using PRN232.LMS.API.Models;
using PRN232.LMS.API.RequestModels;
using PRN232.LMS.Services.Models;
using PRN232.LMS.Services.Services;
namespace PRN232.LMS.API.Controllers;

[ApiController]
[Route("api/students")]
public class StudentsController(ILmsService service) : ControllerBase
{
    [HttpGet] public async Task<IActionResult> GetAll([FromQuery] ListQueryRequest query) => Ok(CollectionResponse<object>.From(await service.GetStudentsAsync(query.ToServiceQuery()), "Students retrieved successfully."));
    [HttpGet("{id:int}")] public async Task<IActionResult> Get(int id) { var x = await service.GetStudentAsync(id); return x is null ? NotFound(ApiResponse<object>.Fail("Student not found.")) : Ok(ApiResponse<StudentResponse>.Ok(x)); }
    [HttpPost] public async Task<IActionResult> Create(StudentRequest request) { var (x, error) = await service.CreateStudentAsync(request); return error is not null ? BadRequest(ApiResponse<object>.Fail(error)) : CreatedAtAction(nameof(Get), new { id = x!.StudentId }, ApiResponse<StudentResponse>.Ok(x, "Student created successfully.")); }
    [HttpPut("{id:int}")] public async Task<IActionResult> Update(int id, StudentRequest request) { var (found, error) = await service.UpdateStudentAsync(id, request); if (!found) return NotFound(ApiResponse<object>.Fail("Student not found.")); return error is null ? Ok(ApiResponse<object>.Ok(new { id }, "Student updated successfully.")) : BadRequest(ApiResponse<object>.Fail(error)); }
    [HttpDelete("{id:int}")] public async Task<IActionResult> Delete(int id) => await service.DeleteStudentAsync(id) ? Ok(ApiResponse<object>.Ok(new { id }, "Student deleted successfully.")) : NotFound(ApiResponse<object>.Fail("Student not found."));
}
