namespace PRN232.LMS.Services.BusinessModels;

public record StudentModel(int StudentId, string FullName, string Email, DateTime DateOfBirth);
public record CourseModel(int CourseId, string CourseName, int SemesterId, int SubjectId);
