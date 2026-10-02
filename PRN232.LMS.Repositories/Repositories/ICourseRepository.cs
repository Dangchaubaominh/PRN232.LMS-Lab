using PRN232.LMS.Repositories.Entities;
using PRN232.LMS.Repositories.Queries;

namespace PRN232.LMS.Repositories.Repositories;

public interface ICourseRepository : IPagedRepository<Course, CourseQuery>
{
    /// <summary>Read-only, with its semester, subject and enrollments.</summary>
    Task<Course?> GetWithDetailsAsync(int id);
}
