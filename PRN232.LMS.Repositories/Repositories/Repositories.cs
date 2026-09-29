using Microsoft.EntityFrameworkCore;
using PRN232.LMS.Repositories.Data;
using PRN232.LMS.Repositories.Entities;

namespace PRN232.LMS.Repositories.Repositories;

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

public class LmsRepository(LmsDbContext context) : ILmsRepository
{
    public IQueryable<Semester> Semesters => context.Semesters.AsNoTracking();
    public IQueryable<Course> Courses => context.Courses.AsNoTracking();
    public IQueryable<Subject> Subjects => context.Subjects.AsNoTracking();
    public IQueryable<Student> Students => context.Students.AsNoTracking();
    public IQueryable<Enrollment> Enrollments => context.Enrollments.AsNoTracking();
    public Task<T?> FindAsync<T>(int id) where T : class => context.Set<T>().FindAsync(id).AsTask();
    public Task AddAsync<T>(T entity) where T : class => context.Set<T>().AddAsync(entity).AsTask();
    public void Remove<T>(T entity) where T : class => context.Set<T>().Remove(entity);
    public Task SaveChangesAsync() => context.SaveChangesAsync();
}
