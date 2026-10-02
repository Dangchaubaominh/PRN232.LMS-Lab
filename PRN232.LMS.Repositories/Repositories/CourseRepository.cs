using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using PRN232.LMS.Repositories.Data;
using PRN232.LMS.Repositories.Entities;
using PRN232.LMS.Repositories.Queries;

namespace PRN232.LMS.Repositories.Repositories;

public class CourseRepository(LmsDbContext context) : Repository<Course>(context), ICourseRepository
{
    private static readonly Dictionary<string, Expression<Func<Course, object>>> SortKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        ["courseId"] = x => x.CourseId,
        ["courseName"] = x => x.CourseName,
        ["semesterId"] = x => x.SemesterId,
        ["subjectId"] = x => x.SubjectId
    };

    public IReadOnlyCollection<string> SortFields => SortKeys.Keys;

    public Task<PagedEntities<Course>> GetPageAsync(CourseQuery query, PageRequest page)
    {
        var courses = Query.Where(x =>
            (query.Search == null || x.CourseName.Contains(query.Search))
            && (query.SemesterId == null || x.SemesterId == query.SemesterId)
            && (query.SubjectId == null || x.SubjectId == query.SubjectId));
        courses = courses.ApplySort(page.Sort, SortKeys, x => x.CourseId);
        if (query.IncludeSemester)
        {
            courses = courses.Include(x => x.Semester);
        }
        if (query.IncludeSubject)
        {
            courses = courses.Include(x => x.Subject);
        }
        if (query.IncludeEnrollments)
        {
            courses = courses.Include(x => x.Enrollments);
        }

        return courses.ToPageAsync(page);
    }

    public Task<Course?> GetWithDetailsAsync(int id) =>
        Query
            .Include(x => x.Semester)
            .Include(x => x.Subject)
            .Include(x => x.Enrollments)
            .SingleOrDefaultAsync(x => x.CourseId == id);
}
