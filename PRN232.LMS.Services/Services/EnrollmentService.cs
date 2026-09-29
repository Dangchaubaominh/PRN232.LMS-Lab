using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using PRN232.LMS.Repositories.Entities;
using PRN232.LMS.Repositories.Repositories;
using PRN232.LMS.Services.BusinessModels;
using PRN232.LMS.Services.Exceptions;
using PRN232.LMS.Services.Mappings;
using PRN232.LMS.Services.Queries;

namespace PRN232.LMS.Services.Services;

public class EnrollmentService(ILmsRepository repository) : IEnrollmentService
{
    private const string NotFoundMessage = "Enrollment not found.";

    private static readonly Dictionary<string, Expression<Func<Enrollment, object>>> SortKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        ["enrollmentId"] = x => x.EnrollmentId,
        ["studentId"] = x => x.StudentId,
        ["courseId"] = x => x.CourseId,
        ["enrollDate"] = x => x.EnrollDate,
        ["status"] = x => x.Status
    };

    private static readonly string[] Expandable = ["student", "course"];

    public Task<PagedResult<EnrollmentModel>> GetAllAsync(ListQuery query)
    {
        QueryHelper.EnsureSupported(query, SortKeys, Expandable);
        var expand = QueryHelper.SplitList(query.Expand);
        var includeStudent = expand.Contains("student");
        var includeCourse = expand.Contains("course");

        var enrollments = repository.Enrollments.Where(x =>
            (query.Search == null || x.Status.Contains(query.Search))
            && (query.Status == null || x.Status == query.Status)
            && (query.StudentId == null || x.StudentId == query.StudentId)
            && (query.CourseId == null || x.CourseId == query.CourseId));
        enrollments = QueryHelper.ApplySort(enrollments, query.Sort, SortKeys, x => x.EnrollmentId);
        if (includeStudent)
        {
            enrollments = enrollments.Include(x => x.Student);
        }
        if (includeCourse)
        {
            enrollments = enrollments.Include(x => x.Course);
        }

        return QueryHelper.ToPagedResultAsync(enrollments, query, x => x.ToModel() with
        {
            Student = includeStudent ? x.Student.ToModel() : null,
            Course = includeCourse ? x.Course.ToModel() : null
        });
    }

    public async Task<EnrollmentModel> GetByIdAsync(int id)
    {
        var enrollment = await repository.Enrollments
            .Include(x => x.Student)
            .Include(x => x.Course).ThenInclude(c => c.Semester)
            .Include(x => x.Course).ThenInclude(c => c.Subject)
            .SingleOrDefaultAsync(x => x.EnrollmentId == id)
            ?? throw new NotFoundException(NotFoundMessage);

        return enrollment.ToModel() with
        {
            Student = enrollment.Student.ToModel(),
            Course = enrollment.Course.ToModel() with
            {
                Semester = enrollment.Course.Semester.ToModel(),
                Subject = enrollment.Course.Subject.ToModel()
            }
        };
    }

    public async Task<EnrollmentModel> CreateAsync(EnrollmentModel enrollment)
    {
        await EnsureReferencesExistAsync(enrollment);
        await EnsureNotEnrolledAsync(enrollment, excludeEnrollmentId: null);

        var entity = new Enrollment
        {
            StudentId = enrollment.StudentId,
            CourseId = enrollment.CourseId,
            EnrollDate = enrollment.EnrollDate,
            Status = enrollment.Status
        };
        await repository.AddAsync(entity);
        await repository.SaveChangesAsync();

        return entity.ToModel();
    }

    public async Task UpdateAsync(int id, EnrollmentModel enrollment)
    {
        var entity = await repository.FindAsync<Enrollment>(id) ?? throw new NotFoundException(NotFoundMessage);
        await EnsureReferencesExistAsync(enrollment);
        await EnsureNotEnrolledAsync(enrollment, excludeEnrollmentId: id);

        entity.StudentId = enrollment.StudentId;
        entity.CourseId = enrollment.CourseId;
        entity.EnrollDate = enrollment.EnrollDate;
        entity.Status = enrollment.Status;
        await repository.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await repository.FindAsync<Enrollment>(id) ?? throw new NotFoundException(NotFoundMessage);

        repository.Remove(entity);
        await repository.SaveChangesAsync();
    }

    private async Task EnsureReferencesExistAsync(EnrollmentModel enrollment)
    {
        var studentExists = await repository.Students.AnyAsync(x => x.StudentId == enrollment.StudentId);
        var courseExists = await repository.Courses.AnyAsync(x => x.CourseId == enrollment.CourseId);
        if (!studentExists || !courseExists)
        {
            throw new BusinessRuleException("StudentId or CourseId does not exist.");
        }
    }

    private async Task EnsureNotEnrolledAsync(EnrollmentModel enrollment, int? excludeEnrollmentId)
    {
        var alreadyEnrolled = await repository.Enrollments.AnyAsync(x =>
            x.StudentId == enrollment.StudentId
            && x.CourseId == enrollment.CourseId
            && (excludeEnrollmentId == null || x.EnrollmentId != excludeEnrollmentId));
        if (alreadyEnrolled)
        {
            throw new BusinessRuleException("Student is already enrolled in this course.");
        }
    }
}
