using PRN232.LMS.Repositories.Entities;
using PRN232.LMS.Repositories.Queries;

namespace PRN232.LMS.Repositories.Repositories;

public interface ISubjectRepository : IPagedRepository<Subject, SubjectQuery>
{
    /// <summary>Read-only, with its courses.</summary>
    Task<Subject?> GetWithCoursesAsync(int id);

    Task<bool> HasCoursesAsync(int subjectId);

    Task<bool> CodeExistsAsync(string subjectCode, int? excludeSubjectId);
}
