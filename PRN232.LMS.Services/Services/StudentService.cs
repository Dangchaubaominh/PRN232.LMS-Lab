using PRN232.LMS.Repositories.Entities;
using PRN232.LMS.Repositories.Queries;
using PRN232.LMS.Repositories.Repositories;
using PRN232.LMS.Services.BusinessModels;
using PRN232.LMS.Services.Exceptions;
using PRN232.LMS.Services.Mappings;
using PRN232.LMS.Services.Queries;

namespace PRN232.LMS.Services.Services;

public class StudentService(IStudentRepository students, ICourseRepository courses, IEnrollmentRepository enrollments) : IStudentService
{
    private const string NotFoundMessage = "Student not found.";

    private static readonly string[] Expandable = ["enrollments"];

    public async Task<PagedResult<StudentModel>> GetAllAsync(ListQuery query, bool includeEnrollmentCount = false)
    {
        var page = QueryOptions.ToPageRequest(query, students.SortFields, Expandable);

        var result = await GetPageAsync(new StudentQuery(query.Search, IncludesEnrollments(query)), page);
        return includeEnrollmentCount ? await WithEnrollmentCountsAsync(result) : result;
    }

    public async Task<PagedResult<StudentModel>> GetByCourseAsync(int courseId, ListQuery query)
    {
        var page = QueryOptions.ToPageRequest(query, students.SortFields, Expandable);
        if (!await courses.ExistsAsync(courseId))
        {
            throw new NotFoundException("Course not found.");
        }

        var studentQuery = new StudentQuery(query.Search, IncludesEnrollments(query), EnrolledInCourseId: courseId, EnrollmentStatus: query.Status);
        return await GetPageAsync(studentQuery, page);
    }

    public async Task<StudentModel> GetByIdAsync(int id)
    {
        var student = await students.GetWithEnrollmentsAsync(id) ?? throw new NotFoundException(NotFoundMessage);

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
        await students.AddAsync(entity);
        await students.SaveChangesAsync();

        return entity.ToModel();
    }

    public async Task UpdateAsync(int id, StudentModel student)
    {
        var entity = await students.FindAsync(id) ?? throw new NotFoundException(NotFoundMessage);
        EnsureBornInThePast(student);
        var code = NormalizeCode(student.StudentCode);
        var email = NormalizeEmail(student.Email);
        await EnsureUniqueAsync(code, email, excludeStudentId: id);

        entity.StudentCode = code;
        entity.FullName = student.FullName.Trim();
        entity.Email = email;
        entity.Phone = NormalizePhone(student.Phone);
        entity.DateOfBirth = student.DateOfBirth;
        await students.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await students.FindAsync(id) ?? throw new NotFoundException(NotFoundMessage);

        students.Remove(entity);
        await students.SaveChangesAsync();
    }

    private static bool IncludesEnrollments(ListQuery query) => QueryOptions.SplitList(query.Expand).Contains("enrollments");

    private async Task<PagedResult<StudentModel>> GetPageAsync(StudentQuery query, PageRequest page)
    {
        var result = await students.GetPageAsync(query, page);
        return result.ToPagedResult(page, x => x.ToModel() with
        {
            Enrollments = query.IncludeEnrollments ? x.Enrollments.Select(e => e.ToModel()).ToList() : null
        });
    }

    private async Task<PagedResult<StudentModel>> WithEnrollmentCountsAsync(PagedResult<StudentModel> page)
    {
        var counts = await enrollments.CountByStudentAsync(page.Items.Select(x => x.StudentId).ToList());
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
        if (await students.CodeExistsAsync(code, excludeStudentId))
        {
            throw new BusinessRuleException("StudentCode already exists.");
        }
        if (await students.EmailExistsAsync(email, excludeStudentId))
        {
            throw new BusinessRuleException("Email already exists.");
        }
    }
}
