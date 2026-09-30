namespace PRN232.LMS.Services.Security;

/// <summary>Values of User.Role, used in [Authorize(Roles = ...)] and in the JWT "role" claim.</summary>
public static class Roles
{
    public const string Admin = "Admin";
    public const string Student = "Student";
}
