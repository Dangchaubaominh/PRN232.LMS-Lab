using PRN232.LMS.API.ResponseModels;
using PRN232.LMS.API.Shaping;
using PRN232.LMS.Services.BusinessModels;

namespace PRN232.LMS.API.Mappings;

/// <summary>
/// Business model to response model mappings. A related resource is mapped only when the service
/// loaded it; otherwise it stays null and is left out of the JSON.
/// </summary>
public static class ResponseMappings
{
    public static SemesterResponse ToResponse(this SemesterModel model) => new()
    {
        SemesterId = model.SemesterId,
        SemesterName = model.SemesterName,
        StartDate = model.StartDate,
        EndDate = model.EndDate,
        Courses = model.Courses?.Select(c => c.ToResponse()).ToList()
    };

    public static SubjectResponse ToResponse(this SubjectModel model) => new()
    {
        SubjectId = model.SubjectId,
        SubjectCode = model.SubjectCode,
        SubjectName = model.SubjectName,
        Credit = model.Credit,
        Courses = model.Courses?.Select(c => c.ToResponse()).ToList()
    };

    public static CourseResponse ToResponse(this CourseModel model) => new()
    {
        CourseId = model.CourseId,
        CourseName = model.CourseName,
        SemesterId = model.SemesterId,
        SubjectId = model.SubjectId,
        Semester = model.Semester?.ToResponse(),
        Subject = model.Subject?.ToResponse(),
        Enrollments = model.Enrollments?.Select(e => e.ToResponse()).ToList()
    };

    public static StudentResponse ToResponse(this StudentModel model) => new()
    {
        StudentId = model.StudentId,
        StudentCode = model.StudentCode,
        FullName = model.FullName,
        Email = model.Email,
        Phone = model.Phone,
        DateOfBirth = model.DateOfBirth,
        Enrollments = model.Enrollments?.Select(e => e.ToResponse()).ToList()
    };

    public static StudentResponseV2 ToResponseV2(this StudentModel model) => new()
    {
        StudentId = model.StudentId,
        StudentCode = model.StudentCode,
        FullName = model.FullName,
        Email = model.Email,
        Phone = model.Phone,
        DateOfBirth = DateOnly.FromDateTime(model.DateOfBirth),
        // A student that was just created has no enrollments, so no count was computed.
        EnrollmentCount = model.EnrollmentCount ?? 0
    };

    public static EnrollmentResponse ToResponse(this EnrollmentModel model) => new()
    {
        EnrollmentId = model.EnrollmentId,
        StudentId = model.StudentId,
        CourseId = model.CourseId,
        EnrollDate = model.EnrollDate,
        Status = model.Status,
        Student = model.Student?.ToResponse(),
        Course = model.Course?.ToResponse()
    };

    public static CollectionResponse<object> ToCollectionResponse<TModel, TResponse>(
        this PagedResult<TModel> page,
        Func<TModel, TResponse> toResponse,
        FieldSelection<TResponse> fields,
        string message) where TResponse : notnull
    {
        var items = page.Items.Select(item => fields.Apply(toResponse(item))).ToList();
        var pagination = new PaginationResponse(page.Page, page.PageSize, page.TotalItems, page.TotalPages);

        return new CollectionResponse<object>(true, message, items, null, pagination);
    }
}
