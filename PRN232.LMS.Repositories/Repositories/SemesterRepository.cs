using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using PRN232.LMS.Repositories.Data;
using PRN232.LMS.Repositories.Entities;
using PRN232.LMS.Repositories.Queries;

namespace PRN232.LMS.Repositories.Repositories;

public class SemesterRepository(LmsDbContext context) : Repository<Semester>(context), ISemesterRepository
{
    private static readonly Dictionary<string, Expression<Func<Semester, object>>> SortKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        ["semesterId"] = x => x.SemesterId,
        ["semesterName"] = x => x.SemesterName,
        ["startDate"] = x => x.StartDate,
        ["endDate"] = x => x.EndDate
    };

    public IReadOnlyCollection<string> SortFields => SortKeys.Keys;

    public Task<PagedEntities<Semester>> GetPageAsync(SemesterQuery query, PageRequest page)
    {
        var semesters = Query.Where(x => query.Search == null || x.SemesterName.Contains(query.Search));
        semesters = semesters.ApplySort(page.Sort, SortKeys, x => x.SemesterId);
        if (query.IncludeCourses)
        {
            semesters = semesters.Include(x => x.Courses);
        }

        return semesters.ToPageAsync(page);
    }

    public Task<Semester?> GetWithCoursesAsync(int id) =>
        Query.Include(x => x.Courses).SingleOrDefaultAsync(x => x.SemesterId == id);

    public Task<bool> HasCoursesAsync(int semesterId) =>
        Context.Courses.AnyAsync(x => x.SemesterId == semesterId);
}
