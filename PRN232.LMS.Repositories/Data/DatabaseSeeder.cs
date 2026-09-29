using Microsoft.EntityFrameworkCore;
using PRN232.LMS.Repositories.Entities;
namespace PRN232.LMS.Repositories.Data;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(LmsDbContext db)
    {
        Exception? lastError = null;
        for (var attempt = 1; attempt <= 10; attempt++)
        {
            try
            {
                await db.Database.EnsureCreatedAsync();
                lastError = null;
                break;
            }
            catch (Exception ex) when (attempt < 10)
            {
                lastError = ex;
                await Task.Delay(TimeSpan.FromSeconds(5));
            }
        }
        if (lastError is not null) throw lastError;
        if (await db.Students.AnyAsync()) return;
        var semesters = Enumerable.Range(0, 5).Select(i => new Semester { SemesterName = $"Semester {i + 1} - 2026", StartDate = new DateTime(2026, 1, 5).AddMonths(i * 2), EndDate = new DateTime(2026, 2, 28).AddMonths(i * 2) }).ToList();
        var subjects = Enumerable.Range(1, 10).Select(i => new Subject { SubjectCode = $"SUB{i:000}", SubjectName = $"Subject {i}", Credit = 2 + i % 3 }).ToList();
        var students = Enumerable.Range(1, 50).Select(i => new Student { FullName = $"Student {i:00}", Email = $"student{i:00}@lms.edu.vn", DateOfBirth = new DateTime(2000 + i % 5, i % 12 + 1, i % 27 + 1) }).ToList();
        db.AddRange(semesters); db.AddRange(subjects); db.AddRange(students); await db.SaveChangesAsync();
        var courses = Enumerable.Range(0, 20).Select(i => new Course { CourseName = $"{subjects[i % 10].SubjectName} - Class {i / 10 + 1}", SemesterId = semesters[i % 5].SemesterId, SubjectId = subjects[i % 10].SubjectId }).ToList();
        db.AddRange(courses); await db.SaveChangesAsync();
        var statuses = new[] { "Active", "Completed", "Dropped" };
        var enrollments = new List<Enrollment>();
        for (var s = 0; s < students.Count; s++) for (var j = 0; j < 10; j++) enrollments.Add(new Enrollment { StudentId = students[s].StudentId, CourseId = courses[(s + j) % courses.Count].CourseId, EnrollDate = new DateTime(2026, 1, 6).AddDays((s * 10 + j) % 200), Status = statuses[(s + j) % statuses.Length] });
        db.AddRange(enrollments); await db.SaveChangesAsync();
    }
}
