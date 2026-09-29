namespace PRN232.LMS.API.ResponseModels;

public record StudentResponse(int StudentId, string FullName, string Email, DateTime DateOfBirth, [property: System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)] object? Enrollments = null);
