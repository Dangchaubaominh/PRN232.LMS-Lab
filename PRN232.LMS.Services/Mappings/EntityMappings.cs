using PRN232.LMS.Repositories.Entities;
using PRN232.LMS.Services.BusinessModels;

namespace PRN232.LMS.Services.Mappings;

/// <summary>
/// Entity to business model mappings. They copy scalar columns only: navigation properties are
/// attached explicitly by each query, because EF fixes up both directions of an Include and a
/// recursive mapping would loop (student -> enrollments -> student -> ...).
/// </summary>
internal static class EntityMappings
{
    public static SemesterModel ToModel(this Semester entity) =>
        new(entity.SemesterId, entity.SemesterName, entity.StartDate, entity.EndDate);

    public static SubjectModel ToModel(this Subject entity) =>
        new(entity.SubjectId, entity.SubjectCode, entity.SubjectName, entity.Credit);

    public static CourseModel ToModel(this Course entity) =>
        new(entity.CourseId, entity.CourseName, entity.SemesterId, entity.SubjectId);

    public static StudentModel ToModel(this Student entity) =>
        new(entity.StudentId, entity.FullName, entity.Email, entity.DateOfBirth);

    public static EnrollmentModel ToModel(this Enrollment entity) =>
        new(entity.EnrollmentId, entity.StudentId, entity.CourseId, entity.EnrollDate, entity.Status);
}
