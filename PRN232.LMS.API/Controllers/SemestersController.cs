using Microsoft.AspNetCore.Mvc;
using PRN232.LMS.API.Mappings;
using PRN232.LMS.API.RequestModels;
using PRN232.LMS.API.ResponseModels;
using PRN232.LMS.API.Shaping;
using PRN232.LMS.Services.Services;

namespace PRN232.LMS.API.Controllers;

[ApiController]
[Route("api/semesters")]
public class SemestersController(ISemesterService semesterService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<CollectionResponse<object>>> GetAll([FromQuery] ListQueryRequest query)
    {
        var fields = FieldSelection<SemesterResponse>.Parse(query.Fields);
        var semesters = await semesterService.GetAllAsync(query.ToListQuery());
        return Ok(semesters.ToCollectionResponse(x => x.ToResponse(), fields, "Semesters retrieved successfully."));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<SemesterResponse>>> GetById([FromRoute] int id)
    {
        var semester = await semesterService.GetByIdAsync(id);
        return Ok(ApiResponse<SemesterResponse>.Ok(semester.ToResponse()));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<SemesterResponse>>> Create([FromBody] SemesterRequest request)
    {
        var semester = await semesterService.CreateAsync(request.ToModel());
        return CreatedAtAction(nameof(GetById), new { id = semester.SemesterId },
            ApiResponse<SemesterResponse>.Ok(semester.ToResponse(), "Semester created successfully."));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse<object>>> Update([FromRoute] int id, [FromBody] SemesterRequest request)
    {
        await semesterService.UpdateAsync(id, request.ToModel());
        return Ok(ApiResponse<object>.Ok(new { id }, "Semester updated successfully."));
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete([FromRoute] int id)
    {
        await semesterService.DeleteAsync(id);
        return Ok(ApiResponse<object>.Ok(new { id }, "Semester deleted successfully."));
    }
}
