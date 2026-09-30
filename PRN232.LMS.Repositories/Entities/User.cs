using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace PRN232.LMS.Repositories.Entities;

public class User
{
    public int UserId { get; set; }

    [MaxLength(50), Unicode(false)]
    public string Username { get; set; } = string.Empty;

    /// <summary>BCrypt hash; the password itself is never stored.</summary>
    [MaxLength(255), Unicode(false)]
    public string PasswordHash { get; set; } = string.Empty;

    [MaxLength(20), Unicode(false)]
    public string Role { get; set; } = string.Empty;

    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
}
