using PRN232.LMS.Repositories.Entities;
using PRN232.LMS.Repositories.Queries;

namespace PRN232.LMS.Repositories.Repositories;

public interface ISemesterRepository : IPagedRepository<Semester, SemesterQuery>
{
    /// <summary>Read-only, with its courses.</summary>
    Task<Semester?> GetWithCoursesAsync(int id);

    Task<bool> HasCoursesAsync(int semesterId);
}
