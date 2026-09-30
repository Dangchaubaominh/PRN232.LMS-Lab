namespace PRN232.LMS.Services.Security;

/// <summary>BCrypt with a per-password random salt (stored inside the hash) and work factor 11.</summary>
public class BCryptPasswordHasher : IPasswordHasher
{
    private const int WorkFactor = 11;

    public string Hash(string password) => BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);

    public bool Verify(string password, string passwordHash) => BCrypt.Net.BCrypt.Verify(password, passwordHash);
}
