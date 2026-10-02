using Microsoft.EntityFrameworkCore;
using PRN232.LMS.Repositories.Entities;
using PRN232.LMS.Repositories.Exceptions;

namespace PRN232.LMS.Repositories.Data;

public class LmsDbContext(DbContextOptions<LmsDbContext> options) : DbContext(options)
{
    public DbSet<Semester> Semesters => Set<Semester>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<Subject> Subjects => Set<Subject>();
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    /// <summary>Saves, reporting unique-index and foreign-key violations as <see cref="DataConflictException"/>.</summary>
    public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        try
        {
            return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }
        catch (DbUpdateException exception) when (DataConflictException.From(exception) is { } conflict)
        {
            throw conflict;
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().HasIndex(x => x.Username).IsUnique();
        modelBuilder.Entity<RefreshToken>().HasIndex(x => x.TokenHash).IsUnique();
        modelBuilder.Entity<RefreshToken>().HasOne(x => x.User).WithMany(x => x.RefreshTokens).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Student>().HasIndex(x => x.Email).IsUnique();
        modelBuilder.Entity<Student>().HasIndex(x => x.StudentCode).IsUnique();
        modelBuilder.Entity<Subject>().HasIndex(x => x.SubjectCode).IsUnique();
        modelBuilder.Entity<Course>().HasOne(x => x.Semester).WithMany(x => x.Courses).HasForeignKey(x => x.SemesterId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Course>().HasOne(x => x.Subject).WithMany(x => x.Courses).HasForeignKey(x => x.SubjectId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Enrollment>().HasOne(x => x.Student).WithMany(x => x.Enrollments).HasForeignKey(x => x.StudentId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<Enrollment>().HasOne(x => x.Course).WithMany(x => x.Enrollments).HasForeignKey(x => x.CourseId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<Enrollment>().HasIndex(x => new { x.StudentId, x.CourseId }).IsUnique();
    }
}
