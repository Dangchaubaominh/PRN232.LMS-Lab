using PRN232.LMS.Repositories.Entities;
using PRN232.LMS.Repositories.Queries;
using PRN232.LMS.Repositories.Repositories;
using PRN232.LMS.Services.BusinessModels;
using PRN232.LMS.Services.Exceptions;
using PRN232.LMS.Services.Mappings;
using PRN232.LMS.Services.Queries;

namespace PRN232.LMS.Services.Services;

public class CourseService(ICourseRepository courses, ISemesterRepository semesters, ISubjectRepository subjects) : ICourseService
{
    private const string NotFoundMessage = "Course not found.";

    private static readonly string[] Expandable = ["semester", "subject", "enrollments"];

    public async Task<PagedResult<CourseModel>> GetAllAsync(ListQuery query)
    {
        var page = QueryOptions.ToPageRequest(query, courses.SortFields, Expandable);
        var expand = QueryOptions.SplitList(query.Expand);
        var courseQuery = new CourseQuery(
            query.Search,
            query.SemesterId,
            query.SubjectId,
            IncludeSemester: expand.Contains("semester"),
            IncludeSubject: expand.Contains("subject"),
            IncludeEnrollments: expand.Contains("enrollments"));

        var result = await courses.GetPageAsync(courseQuery, page);
        return result.ToPagedResult(page, x => x.ToModel() with
        {
            Semester = courseQuery.IncludeSemester ? x.Semester.ToModel() : null,
            Subject = courseQuery.IncludeSubject ? x.Subject.ToModel() : null,
            Enrollments = courseQuery.IncludeEnrollments ? x.Enrollments.Select(e => e.ToModel()).ToList() : null
        });
    }

    public async Task<CourseModel> GetByIdAsync(int id)
    {
        var course = await courses.GetWithDetailsAsync(id) ?? throw new NotFoundException(NotFoundMessage);

        return course.ToModel() with
        {
            Semester = course.Semester.ToModel(),
            Subject = course.Subject.ToModel(),
            Enrollments = course.Enrollments.Select(e => e.ToModel()).ToList()
        };
    }

    public async Task<CourseModel> CreateAsync(CourseModel course)
    {
        await EnsureReferencesExistAsync(course);

        var entity = new Course
        {
            CourseName = course.CourseName.Trim(),
            SemesterId = course.SemesterId,
            SubjectId = course.SubjectId
        };
        await courses.AddAsync(entity);
        await courses.SaveChangesAsync();

        return entity.ToModel();
    }

    public async Task UpdateAsync(int id, CourseModel course)
    {
        var entity = await courses.FindAsync(id) ?? throw new NotFoundException(NotFoundMessage);
        await EnsureReferencesExistAsync(course);

        entity.CourseName = course.CourseName.Trim();
        entity.SemesterId = course.SemesterId;
        entity.SubjectId = course.SubjectId;
        await courses.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await courses.FindAsync(id) ?? throw new NotFoundException(NotFoundMessage);

        courses.Remove(entity);
        await courses.SaveChangesAsync();
    }

    private async Task EnsureReferencesExistAsync(CourseModel course)
    {
        if (!await semesters.ExistsAsync(course.SemesterId) || !await subjects.ExistsAsync(course.SubjectId))
        {
            throw new BusinessRuleException("SemesterId or SubjectId does not exist.");
        }
    }
}
