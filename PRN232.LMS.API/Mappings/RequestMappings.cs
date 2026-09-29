using PRN232.LMS.API.RequestModels;
using PRN232.LMS.Services.BusinessModels;

namespace PRN232.LMS.API.Mappings;

/// <summary>Request model to business model mappings. Ids are 0: the route or the database supplies them.</summary>
public static class RequestMappings
{
    /// <remarks>SemesterName is non-null once <c>SemesterRequestValidator</c> has passed.</remarks>
    public static SemesterModel ToModel(this SemesterRequest request) =>
        new(0, request.SemesterName!, request.StartDate, request.EndDate);

    public static SubjectModel ToModel(this SubjectRequest request) =>
        new(0, request.SubjectCode, request.SubjectName, request.Credit);

    public static CourseModel ToModel(this CourseRequest request) =>
        new(0, request.CourseName, request.SemesterId, request.SubjectId);

    public static StudentModel ToModel(this StudentRequest request) =>
        new(0, request.StudentCode, request.FullName, request.Email, request.Phone, request.DateOfBirth);

    public static EnrollmentModel ToModel(this EnrollmentRequest request) =>
        new(0, request.StudentId, request.CourseId, request.EnrollDate, request.Status);

    /// <summary>Everything except <see cref="StudentQueryRequest.Fields"/>, which the API layer applies itself.</summary>
    public static ListQuery ToListQuery(this StudentQueryRequest request) => new()
    {
        Search = request.Search,
        Sort = request.Sort,
        Page = request.Page,
        Size = request.Size
    };

    /// <summary>Everything except <see cref="ListQueryRequest.Fields"/>, which the API layer applies itself.</summary>
    public static ListQuery ToListQuery(this ListQueryRequest request) => new()
    {
        Search = request.Search,
        Status = request.Status,
        SemesterId = request.SemesterId,
        SubjectId = request.SubjectId,
        StudentId = request.StudentId,
        CourseId = request.CourseId,
        Sort = request.Sort,
        Page = request.Page,
        Size = request.Size,
        Expand = request.Expand
    };
}
