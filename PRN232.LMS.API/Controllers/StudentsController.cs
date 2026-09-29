using Microsoft.AspNetCore.Mvc;
using PRN232.LMS.API.Mappings;
using PRN232.LMS.API.RequestModels;
using PRN232.LMS.API.ResponseModels;
using PRN232.LMS.API.Shaping;
using PRN232.LMS.Services.Services;

namespace PRN232.LMS.API.Controllers;

[ApiController]
[Route("api/students")]
public class StudentsController(IStudentService studentService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<CollectionResponse<object>>> GetAll([FromQuery] ListQueryRequest query)
    {
        var fields = FieldSelection<StudentResponse>.Parse(query.Fields);
        var students = await studentService.GetAllAsync(query.ToListQuery());
        return Ok(students.ToCollectionResponse(x => x.ToResponse(), fields, "Students retrieved successfully."));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<StudentResponse>>> GetById([FromRoute] int id)
    {
        var student = await studentService.GetByIdAsync(id);
        return Ok(ApiResponse<StudentResponse>.Ok(student.ToResponse()));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<StudentResponse>>> Create([FromBody] StudentRequest request)
    {
        var student = await studentService.CreateAsync(request.ToModel());
        return CreatedAtAction(nameof(GetById), new { id = student.StudentId },
            ApiResponse<StudentResponse>.Ok(student.ToResponse(), "Student created successfully."));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse<object>>> Update([FromRoute] int id, [FromBody] StudentRequest request)
    {
        await studentService.UpdateAsync(id, request.ToModel());
        return Ok(ApiResponse<object>.Ok(new { id }, "Student updated successfully."));
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete([FromRoute] int id)
    {
        await studentService.DeleteAsync(id);
        return Ok(ApiResponse<object>.Ok(new { id }, "Student deleted successfully."));
    }
}
