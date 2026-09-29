using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using PRN232.LMS.Repositories.Entities;
using PRN232.LMS.Repositories.Repositories;
using PRN232.LMS.Services.BusinessModels;
using PRN232.LMS.Services.Exceptions;
using PRN232.LMS.Services.Mappings;
using PRN232.LMS.Services.Queries;

namespace PRN232.LMS.Services.Services;

public class StudentService(ILmsRepository repository) : IStudentService
{
    private const string NotFoundMessage = "Student not found.";

    private static readonly Dictionary<string, Expression<Func<Student, object>>> SortKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        ["studentId"] = x => x.StudentId,
        ["studentCode"] = x => x.StudentCode,
        ["fullName"] = x => x.FullName,
        ["email"] = x => x.Email,
        ["dateOfBirth"] = x => x.DateOfBirth
    };

    private static readonly string[] Expandable = ["enrollments"];

    public async Task<PagedResult<StudentModel>> GetAllAsync(ListQuery query, bool includeEnrollmentCount = false)
    {
        QueryHelper.EnsureSupported(query, SortKeys, Expandable);

        var page = await GetPageAsync(repository.Students, query);
        return includeEnrollmentCount ? await WithEnrollmentCountsAsync(page) : page;
    }

    public async Task<PagedResult<StudentModel>> GetByCourseAsync(int courseId, ListQuery query)
    {
        QueryHelper.EnsureSupported(query, SortKeys, Expandable);
        if (!await repository.Courses.AnyAsync(x => x.CourseId == courseId))
        {
            throw new NotFoundException("Course not found.");
        }

        var enrolled = repository.Students.Where(x => x.Enrollments.Any(e =>
            e.CourseId == courseId && (query.Status == null || e.Status == query.Status)));
        return await GetPageAsync(enrolled, query);
    }

    public async Task<StudentModel> GetByIdAsync(int id)
    {
        var student = await repository.Students
            .Include(x => x.Enrollments).ThenInclude(e => e.Course)
            .SingleOrDefaultAsync(x => x.StudentId == id)
            ?? throw new NotFoundException(NotFoundMessage);

        return student.ToModel() with
        {
            Enrollments = student.Enrollments.Select(e => e.ToModel() with { Course = e.Course.ToModel() }).ToList(),
            EnrollmentCount = student.Enrollments.Count
        };
    }

    public async Task<StudentModel> CreateAsync(StudentModel student)
    {
        EnsureBornInThePast(student);
        var code = NormalizeCode(student.StudentCode);
        var email = NormalizeEmail(student.Email);
        await EnsureUniqueAsync(code, email, excludeStudentId: null);

        var entity = new Student
        {
            StudentCode = code,
            FullName = student.FullName.Trim(),
            Email = email,
            Phone = NormalizePhone(student.Phone),
            DateOfBirth = student.DateOfBirth
        };
        await repository.AddAsync(entity);
        await repository.SaveChangesAsync();

        return entity.ToModel();
    }

    public async Task UpdateAsync(int id, StudentModel student)
    {
        var entity = await repository.FindAsync<Student>(id) ?? throw new NotFoundException(NotFoundMessage);
        EnsureBornInThePast(student);
        var code = NormalizeCode(student.StudentCode);
        var email = NormalizeEmail(student.Email);
        await EnsureUniqueAsync(code, email, excludeStudentId: id);

        entity.StudentCode = code;
        entity.FullName = student.FullName.Trim();
        entity.Email = email;
        entity.Phone = NormalizePhone(student.Phone);
        entity.DateOfBirth = student.DateOfBirth;
        await repository.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await repository.FindAsync<Student>(id) ?? throw new NotFoundException(NotFoundMessage);

        repository.Remove(entity);
        await repository.SaveChangesAsync();
    }

    /// <summary>Search, sort, page and expand over <paramref name="students"/>.</summary>
    private static Task<PagedResult<StudentModel>> GetPageAsync(IQueryable<Student> students, ListQuery query)
    {
        var includeEnrollments = QueryHelper.SplitList(query.Expand).Contains("enrollments");

        students = students.Where(x =>
            query.Search == null
            || x.StudentCode.Contains(query.Search)
            || x.FullName.Contains(query.Search)
            || x.Email.Contains(query.Search));
        students = QueryHelper.ApplySort(students, query.Sort, SortKeys, x => x.StudentId);
        if (includeEnrollments)
        {
            students = students.Include(x => x.Enrollments);
        }

        return QueryHelper.ToPagedResultAsync(students, query, x => x.ToModel() with
        {
            Enrollments = includeEnrollments ? x.Enrollments.Select(e => e.ToModel()).ToList() : null
        });
    }

    /// <summary>Counts the enrollments of the students on the page with one grouped query.</summary>
    private async Task<PagedResult<StudentModel>> WithEnrollmentCountsAsync(PagedResult<StudentModel> page)
    {
        var studentIds = page.Items.Select(x => x.StudentId).ToList();
        var counts = await repository.Enrollments
            .Where(e => studentIds.Contains(e.StudentId))
            .GroupBy(e => e.StudentId)
            .Select(g => new { StudentId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.StudentId, x => x.Count);

        return page with
        {
            Items = page.Items.Select(x => x with { EnrollmentCount = counts.GetValueOrDefault(x.StudentId) }).ToList()
        };
    }

    private static string NormalizeCode(string code) => code.Trim().ToUpperInvariant();

    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();

    private static string? NormalizePhone(string? phone) => string.IsNullOrWhiteSpace(phone) ? null : phone.Trim();

    private static void EnsureBornInThePast(StudentModel student)
    {
        if (student.DateOfBirth >= DateTime.Today)
        {
            throw new BusinessRuleException("DateOfBirth must be in the past.");
        }
    }

    private async Task EnsureUniqueAsync(string code, string email, int? excludeStudentId)
    {
        var others = repository.Students.Where(x => excludeStudentId == null || x.StudentId != excludeStudentId);
        if (await others.AnyAsync(x => x.StudentCode == code))
        {
            throw new BusinessRuleException("StudentCode already exists.");
        }
        if (await others.AnyAsync(x => x.Email == email))
        {
            throw new BusinessRuleException("Email already exists.");
        }
    }
}
