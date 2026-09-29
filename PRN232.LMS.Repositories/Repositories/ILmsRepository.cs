using PRN232.LMS.Repositories.Entities;

namespace PRN232.LMS.Repositories.Repositories;

/// <summary>
/// Data access for the LMS tables. Query properties are read-only (no tracking);
/// entities that will be modified must be loaded through <see cref="FindAsync{T}"/>.
/// </summary>
public interface ILmsRepository
{
    IQueryable<Semester> Semesters { get; }
    IQueryable<Course> Courses { get; }
    IQueryable<Subject> Subjects { get; }
    IQueryable<Student> Students { get; }
    IQueryable<Enrollment> Enrollments { get; }

    Task<T?> FindAsync<T>(int id) where T : class;
    Task AddAsync<T>(T entity) where T : class;
    void Remove<T>(T entity) where T : class;
    Task SaveChangesAsync();
}
