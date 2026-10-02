using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using PRN232.LMS.Repositories.Data;
using PRN232.LMS.Repositories.Entities;
using PRN232.LMS.Repositories.Queries;

namespace PRN232.LMS.Repositories.Repositories;

public class EnrollmentRepository(LmsDbContext context) : Repository<Enrollment>(context), IEnrollmentRepository
{
    private static readonly Dictionary<string, Expression<Func<Enrollment, object>>> SortKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        ["enrollmentId"] = x => x.EnrollmentId,
        ["studentId"] = x => x.StudentId,
        ["courseId"] = x => x.CourseId,
        ["enrollDate"] = x => x.EnrollDate,
        ["status"] = x => x.Status
    };

    public IReadOnlyCollection<string> SortFields => SortKeys.Keys;

    public Task<PagedEntities<Enrollment>> GetPageAsync(EnrollmentQuery query, PageRequest page)
    {
        var enrollments = Query.Where(x =>
            (query.Search == null || x.Status.Contains(query.Search))
            && (query.Status == null || x.Status == query.Status)
            && (query.StudentId == null || x.StudentId == query.StudentId)
            && (query.CourseId == null || x.CourseId == query.CourseId));
        enrollments = enrollments.ApplySort(page.Sort, SortKeys, x => x.EnrollmentId);
        if (query.IncludeStudent)
        {
            enrollments = enrollments.Include(x => x.Student);
        }
        if (query.IncludeCourse)
        {
            enrollments = enrollments.Include(x => x.Course);
        }

        return enrollments.ToPageAsync(page);
    }

    public Task<Enrollment?> GetWithDetailsAsync(int id) =>
        Query
            .Include(x => x.Student)
            .Include(x => x.Course).ThenInclude(c => c.Semester)
            .Include(x => x.Course).ThenInclude(c => c.Subject)
            .SingleOrDefaultAsync(x => x.EnrollmentId == id);

    public Task<bool> ExistsForStudentAndCourseAsync(int studentId, int courseId, int? excludeEnrollmentId) =>
        Query.AnyAsync(x =>
            x.StudentId == studentId
            && x.CourseId == courseId
            && (excludeEnrollmentId == null || x.EnrollmentId != excludeEnrollmentId));

    public async Task<IReadOnlyDictionary<int, int>> CountByStudentAsync(IReadOnlyCollection<int> studentIds) =>
        await Query
            .Where(x => studentIds.Contains(x.StudentId))
            .GroupBy(x => x.StudentId)
            .Select(g => new { StudentId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.StudentId, x => x.Count);
}
