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

    public Task<PagedResult<StudentModel>> GetAllAsync(ListQuery query)
    {
        QueryHelper.EnsureSupported(query, SortKeys, Expandable);
        var includeEnrollments = QueryHelper.SplitList(query.Expand).Contains("enrollments");

        var students = repository.Students.Where(x =>
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

    public async Task<StudentModel> GetByIdAsync(int id)
    {
        var student = await repository.Students
            .Include(x => x.Enrollments).ThenInclude(e => e.Course)
            .SingleOrDefaultAsync(x => x.StudentId == id)
            ?? throw new NotFoundException(NotFoundMessage);

        return student.ToModel() with
        {
            Enrollments = student.Enrollments.Select(e => e.ToModel() with { Course = e.Course.ToModel() }).ToList()
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
