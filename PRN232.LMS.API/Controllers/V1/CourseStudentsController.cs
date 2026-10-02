using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PRN232.LMS.API.Mappings;
using PRN232.LMS.API.RequestModels;
using PRN232.LMS.API.ResponseModels;
using PRN232.LMS.API.Shaping;
using PRN232.LMS.Services.Services;

namespace PRN232.LMS.API.Controllers.V1;

/// <summary>Nested resource: the students enrolled in a course.</summary>
[ApiController]
[Authorize]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/courses/{courseId:int}/students")]
public class CourseStudentsController(IStudentService studentService) : ControllerBase
{
    /// <summary>
    /// Students enrolled in the course. Supports search, sort, paging, fields and expand like
    /// GET /students; "status" keeps only students whose enrollment in this course has that status.
    /// </summary>
    [HttpGet(Name = "GetCourseStudents")]
    [ProducesResponseType(typeof(CollectionResponse<StudentResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<CollectionResponse<object>>> GetAll([FromRoute] int courseId, [FromQuery] ListQueryRequest query)
    {
        var fields = FieldSelection<StudentResponse>.Parse(query.Fields);
        var students = await studentService.GetByCourseAsync(courseId, query.ToListQuery());
        return Ok(students.ToCollectionResponse(x => x.ToResponse(), fields, "Course students retrieved successfully."));
    }
}
