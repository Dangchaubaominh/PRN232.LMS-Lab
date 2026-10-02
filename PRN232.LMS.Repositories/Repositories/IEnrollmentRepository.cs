using PRN232.LMS.Repositories.Entities;
using PRN232.LMS.Repositories.Queries;

namespace PRN232.LMS.Repositories.Repositories;

public interface IEnrollmentRepository : IPagedRepository<Enrollment, EnrollmentQuery>
{
    /// <summary>Read-only, with its student and its course's semester and subject.</summary>
    Task<Enrollment?> GetWithDetailsAsync(int id);

    Task<bool> ExistsForStudentAndCourseAsync(int studentId, int courseId, int? excludeEnrollmentId);

    /// <summary>Number of enrollments per student, in one grouped query; students without any are absent.</summary>
    Task<IReadOnlyDictionary<int, int>> CountByStudentAsync(IReadOnlyCollection<int> studentIds);
}
