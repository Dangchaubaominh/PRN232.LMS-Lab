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
        ["fullName"] = x => x.FullName,
        ["email"] = x => x.Email,
        ["dateOfBirth"] = x => x.DateOfBirth
    };

    private static readonly string[] Expandable = ["enrollments"];

    public Task<PagedResult<StudentModel>> GetAllAsync(ListQuery query)
    {
        QueryHelper.EnsureSupported(query, SortKeys, Expandable);
        var includeEnrollments = QueryHelper.SplitList(query.Expand).Contains("enrollments");

        var students = repository.Students
            .Where(x => query.Search == null || x.FullName.Contains(query.Search) || x.Email.Contains(query.Search));
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
        var email = NormalizeEmail(student.Email);
        await EnsureEmailAvailableAsync(email, excludeStudentId: null);

        var entity = new Student
        {
            FullName = student.FullName.Trim(),
            Email = email,
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
        var email = NormalizeEmail(student.Email);
        await EnsureEmailAvailableAsync(email, excludeStudentId: id);

        entity.FullName = student.FullName.Trim();
        entity.Email = email;
        entity.DateOfBirth = student.DateOfBirth;
        await repository.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await repository.FindAsync<Student>(id) ?? throw new NotFoundException(NotFoundMessage);

        repository.Remove(entity);
        await repository.SaveChangesAsync();
    }

    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();

    private static void EnsureBornInThePast(StudentModel student)
    {
        if (student.DateOfBirth >= DateTime.Today)
        {
            throw new BusinessRuleException("DateOfBirth must be in the past.");
        }
    }

    private async Task EnsureEmailAvailableAsync(string email, int? excludeStudentId)
    {
        var taken = await repository.Students
            .AnyAsync(x => x.Email == email && (excludeStudentId == null || x.StudentId != excludeStudentId));
        if (taken)
        {
            throw new BusinessRuleException("Email already exists.");
        }
    }
}
