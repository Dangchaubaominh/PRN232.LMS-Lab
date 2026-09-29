using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using PRN232.LMS.Repositories.Entities;
using PRN232.LMS.Repositories.Repositories;
using PRN232.LMS.Services.BusinessModels;
using PRN232.LMS.Services.Exceptions;
using PRN232.LMS.Services.Mappings;
using PRN232.LMS.Services.Queries;

namespace PRN232.LMS.Services.Services;

public class CourseService(ILmsRepository repository) : ICourseService
{
    private const string NotFoundMessage = "Course not found.";

    private static readonly Dictionary<string, Expression<Func<Course, object>>> SortKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        ["courseId"] = x => x.CourseId,
        ["courseName"] = x => x.CourseName,
        ["semesterId"] = x => x.SemesterId,
        ["subjectId"] = x => x.SubjectId
    };

    private static readonly string[] Expandable = ["semester", "subject", "enrollments"];

    public Task<PagedResult<CourseModel>> GetAllAsync(ListQuery query)
    {
        QueryHelper.EnsureSupported(query, SortKeys, Expandable);
        var expand = QueryHelper.SplitList(query.Expand);
        var includeSemester = expand.Contains("semester");
        var includeSubject = expand.Contains("subject");
        var includeEnrollments = expand.Contains("enrollments");

        var courses = repository.Courses.Where(x =>
            (query.Search == null || x.CourseName.Contains(query.Search))
            && (query.SemesterId == null || x.SemesterId == query.SemesterId)
            && (query.SubjectId == null || x.SubjectId == query.SubjectId));
        courses = QueryHelper.ApplySort(courses, query.Sort, SortKeys, x => x.CourseId);
        if (includeSemester)
        {
            courses = courses.Include(x => x.Semester);
        }
        if (includeSubject)
        {
            courses = courses.Include(x => x.Subject);
        }
        if (includeEnrollments)
        {
            courses = courses.Include(x => x.Enrollments);
        }

        return QueryHelper.ToPagedResultAsync(courses, query, x => x.ToModel() with
        {
            Semester = includeSemester ? x.Semester.ToModel() : null,
            Subject = includeSubject ? x.Subject.ToModel() : null,
            Enrollments = includeEnrollments ? x.Enrollments.Select(e => e.ToModel()).ToList() : null
        });
    }

    public async Task<CourseModel> GetByIdAsync(int id)
    {
        var course = await repository.Courses
            .Include(x => x.Semester)
            .Include(x => x.Subject)
            .Include(x => x.Enrollments)
            .SingleOrDefaultAsync(x => x.CourseId == id)
            ?? throw new NotFoundException(NotFoundMessage);

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
        await repository.AddAsync(entity);
        await repository.SaveChangesAsync();

        return entity.ToModel();
    }

    public async Task UpdateAsync(int id, CourseModel course)
    {
        var entity = await repository.FindAsync<Course>(id) ?? throw new NotFoundException(NotFoundMessage);
        await EnsureReferencesExistAsync(course);

        entity.CourseName = course.CourseName.Trim();
        entity.SemesterId = course.SemesterId;
        entity.SubjectId = course.SubjectId;
        await repository.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await repository.FindAsync<Course>(id) ?? throw new NotFoundException(NotFoundMessage);

        repository.Remove(entity);
        await repository.SaveChangesAsync();
    }

    private async Task EnsureReferencesExistAsync(CourseModel course)
    {
        var semesterExists = await repository.Semesters.AnyAsync(x => x.SemesterId == course.SemesterId);
        var subjectExists = await repository.Subjects.AnyAsync(x => x.SubjectId == course.SubjectId);
        if (!semesterExists || !subjectExists)
        {
            throw new BusinessRuleException("SemesterId or SubjectId does not exist.");
        }
    }
}
