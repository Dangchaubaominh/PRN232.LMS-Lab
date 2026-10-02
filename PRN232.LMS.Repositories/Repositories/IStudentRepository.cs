using PRN232.LMS.Repositories.Entities;
using PRN232.LMS.Repositories.Queries;

namespace PRN232.LMS.Repositories.Repositories;

public interface IStudentRepository : IPagedRepository<Student, StudentQuery>
{
    /// <summary>Read-only, with its enrollments and their courses.</summary>
    Task<Student?> GetWithEnrollmentsAsync(int id);

    Task<bool> CodeExistsAsync(string studentCode, int? excludeStudentId);

    Task<bool> EmailExistsAsync(string email, int? excludeStudentId);
}
