using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using PRN232.LMS.Repositories.Data;
using PRN232.LMS.Repositories.Entities;
using PRN232.LMS.Repositories.Queries;

namespace PRN232.LMS.Repositories.Repositories;

public class StudentRepository(LmsDbContext context) : Repository<Student>(context), IStudentRepository
{
    private static readonly Dictionary<string, Expression<Func<Student, object>>> SortKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        ["studentId"] = x => x.StudentId,
        ["studentCode"] = x => x.StudentCode,
        ["fullName"] = x => x.FullName,
        ["email"] = x => x.Email,
        ["dateOfBirth"] = x => x.DateOfBirth
    };

    public IReadOnlyCollection<string> SortFields => SortKeys.Keys;

    public Task<PagedEntities<Student>> GetPageAsync(StudentQuery query, PageRequest page)
    {
        var students = Query.Where(x =>
            query.Search == null
            || x.StudentCode.Contains(query.Search)
            || x.FullName.Contains(query.Search)
            || x.Email.Contains(query.Search));
        if (query.EnrolledInCourseId is not null)
        {
            students = students.Where(x => x.Enrollments.Any(e =>
                e.CourseId == query.EnrolledInCourseId
                && (query.EnrollmentStatus == null || e.Status == query.EnrollmentStatus)));
        }
        students = students.ApplySort(page.Sort, SortKeys, x => x.StudentId);
        if (query.IncludeEnrollments)
        {
            students = students.Include(x => x.Enrollments);
        }

        return students.ToPageAsync(page);
    }

    public Task<Student?> GetWithEnrollmentsAsync(int id) =>
        Query.Include(x => x.Enrollments).ThenInclude(e => e.Course).SingleOrDefaultAsync(x => x.StudentId == id);

    public Task<bool> CodeExistsAsync(string studentCode, int? excludeStudentId) =>
        Query.AnyAsync(x => x.StudentCode == studentCode && (excludeStudentId == null || x.StudentId != excludeStudentId));

    public Task<bool> EmailExistsAsync(string email, int? excludeStudentId) =>
        Query.AnyAsync(x => x.Email == email && (excludeStudentId == null || x.StudentId != excludeStudentId));
}
