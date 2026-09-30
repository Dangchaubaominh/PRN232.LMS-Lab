using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PRN232.LMS.API.Mappings;
using PRN232.LMS.API.RequestModels;
using PRN232.LMS.API.ResponseModels;
using PRN232.LMS.API.Shaping;
using PRN232.LMS.Services.Security;
using PRN232.LMS.Services.Services;

namespace PRN232.LMS.API.Controllers.V1;

[ApiController]
[Authorize]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/subjects")]
public class SubjectsController(ISubjectService subjectService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<CollectionResponse<object>>> GetAll([FromQuery] ListQueryRequest query)
    {
        var fields = FieldSelection<SubjectResponse>.Parse(query.Fields);
        var subjects = await subjectService.GetAllAsync(query.ToListQuery());
        return Ok(subjects.ToCollectionResponse(x => x.ToResponse(), fields, "Subjects retrieved successfully."));
    }

    [HttpGet("{id:int}", Name = "GetSubjectById")]
    public async Task<ActionResult<ApiResponse<SubjectResponse>>> GetById([FromRoute] int id)
    {
        var subject = await subjectService.GetByIdAsync(id);
        return Ok(ApiResponse<SubjectResponse>.Ok(subject.ToResponse()));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<SubjectResponse>>> Create([FromBody] SubjectRequest request)
    {
        var subject = await subjectService.CreateAsync(request.ToModel());
        return CreatedAtRoute("GetSubjectById", new { id = subject.SubjectId },
            ApiResponse<SubjectResponse>.Ok(subject.ToResponse(), "Subject created successfully."));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse<object>>> Update([FromRoute] int id, [FromBody] SubjectRequest request)
    {
        await subjectService.UpdateAsync(id, request.ToModel());
        return Ok(ApiResponse<object>.Ok(new { id }, "Subject updated successfully."));
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete([FromRoute] int id)
    {
        await subjectService.DeleteAsync(id);
        return Ok(ApiResponse<object>.Ok(new { id }, "Subject deleted successfully."));
    }
}
