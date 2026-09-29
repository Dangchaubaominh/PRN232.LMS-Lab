using PRN232.LMS.Services.BusinessModels;

namespace PRN232.LMS.Services.Services;

public interface IStudentService
{
    /// <param name="includeEnrollmentCount">Fill <see cref="StudentModel.EnrollmentCount"/> (one extra aggregate query).</param>
    Task<PagedResult<StudentModel>> GetAllAsync(ListQuery query, bool includeEnrollmentCount = false);

    /// <summary>
    /// Students enrolled in a course; <see cref="ListQuery.Status"/> filters by enrollment status.
    /// </summary>
    /// <exception cref="Exceptions.NotFoundException">The course does not exist.</exception>
    Task<PagedResult<StudentModel>> GetByCourseAsync(int courseId, ListQuery query);

    /// <summary>Loads the student with enrollments (and their courses) and the enrollment count.</summary>
    Task<StudentModel> GetByIdAsync(int id);

    Task<StudentModel> CreateAsync(StudentModel student);
    Task UpdateAsync(int id, StudentModel student);
    Task DeleteAsync(int id);
}
