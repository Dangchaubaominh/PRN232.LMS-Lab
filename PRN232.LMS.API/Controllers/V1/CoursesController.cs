using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using PRN232.LMS.API.Mappings;
using PRN232.LMS.API.RequestModels;
using PRN232.LMS.API.ResponseModels;
using PRN232.LMS.API.Shaping;
using PRN232.LMS.Services.Services;

namespace PRN232.LMS.API.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/courses")]
public class CoursesController(ICourseService courseService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<CollectionResponse<object>>> GetAll([FromQuery] ListQueryRequest query)
    {
        var fields = FieldSelection<CourseResponse>.Parse(query.Fields);
        var courses = await courseService.GetAllAsync(query.ToListQuery());
        return Ok(courses.ToCollectionResponse(x => x.ToResponse(), fields, "Courses retrieved successfully."));
    }

    [HttpGet("{id:int}", Name = "GetCourseById")]
    public async Task<ActionResult<ApiResponse<CourseResponse>>> GetById([FromRoute] int id)
    {
        var course = await courseService.GetByIdAsync(id);
        return Ok(ApiResponse<CourseResponse>.Ok(course.ToResponse()));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<CourseResponse>>> Create([FromBody] CourseRequest request)
    {
        var course = await courseService.CreateAsync(request.ToModel());
        return CreatedAtRoute("GetCourseById", new { id = course.CourseId },
            ApiResponse<CourseResponse>.Ok(course.ToResponse(), "Course created successfully."));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse<object>>> Update([FromRoute] int id, [FromBody] CourseRequest request)
    {
        await courseService.UpdateAsync(id, request.ToModel());
        return Ok(ApiResponse<object>.Ok(new { id }, "Course updated successfully."));
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete([FromRoute] int id)
    {
        await courseService.DeleteAsync(id);
        return Ok(ApiResponse<object>.Ok(new { id }, "Course deleted successfully."));
    }
}
