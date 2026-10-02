using PRN232.LMS.Repositories.Entities;
using PRN232.LMS.Repositories.Queries;
using PRN232.LMS.Repositories.Repositories;
using PRN232.LMS.Services.BusinessModels;
using PRN232.LMS.Services.Exceptions;
using PRN232.LMS.Services.Mappings;
using PRN232.LMS.Services.Queries;

namespace PRN232.LMS.Services.Services;

public class EnrollmentService(IEnrollmentRepository enrollments, IStudentRepository students, ICourseRepository courses) : IEnrollmentService
{
    private const string NotFoundMessage = "Enrollment not found.";

    private static readonly string[] Expandable = ["student", "course"];

    public async Task<PagedResult<EnrollmentModel>> GetAllAsync(ListQuery query)
    {
        var page = QueryOptions.ToPageRequest(query, enrollments.SortFields, Expandable);
        var expand = QueryOptions.SplitList(query.Expand);
        var enrollmentQuery = new EnrollmentQuery(
            query.Search,
            query.Status,
            query.StudentId,
            query.CourseId,
            IncludeStudent: expand.Contains("student"),
            IncludeCourse: expand.Contains("course"));

        var result = await enrollments.GetPageAsync(enrollmentQuery, page);
        return result.ToPagedResult(page, x => x.ToModel() with
        {
            Student = enrollmentQuery.IncludeStudent ? x.Student.ToModel() : null,
            Course = enrollmentQuery.IncludeCourse ? x.Course.ToModel() : null
        });
    }

    public async Task<EnrollmentModel> GetByIdAsync(int id)
    {
        var enrollment = await enrollments.GetWithDetailsAsync(id) ?? throw new NotFoundException(NotFoundMessage);

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
        await enrollments.AddAsync(entity);
        await enrollments.SaveChangesAsync();

        return entity.ToModel();
    }

    public async Task UpdateAsync(int id, EnrollmentModel enrollment)
    {
        var entity = await enrollments.FindAsync(id) ?? throw new NotFoundException(NotFoundMessage);
        await EnsureReferencesExistAsync(enrollment);
        await EnsureNotEnrolledAsync(enrollment, excludeEnrollmentId: id);

        entity.StudentId = enrollment.StudentId;
        entity.CourseId = enrollment.CourseId;
        entity.EnrollDate = enrollment.EnrollDate;
        entity.Status = enrollment.Status;
        await enrollments.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await enrollments.FindAsync(id) ?? throw new NotFoundException(NotFoundMessage);

        enrollments.Remove(entity);
        await enrollments.SaveChangesAsync();
    }

    private async Task EnsureReferencesExistAsync(EnrollmentModel enrollment)
    {
        if (!await students.ExistsAsync(enrollment.StudentId) || !await courses.ExistsAsync(enrollment.CourseId))
        {
            throw new BusinessRuleException("StudentId or CourseId does not exist.");
        }
    }

    private async Task EnsureNotEnrolledAsync(EnrollmentModel enrollment, int? excludeEnrollmentId)
    {
        if (await enrollments.ExistsForStudentAndCourseAsync(enrollment.StudentId, enrollment.CourseId, excludeEnrollmentId))
        {
            throw new BusinessRuleException("Student is already enrolled in this course.");
        }
    }
}
