using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace PRN232.LMS.Repositories.Entities;

/// <summary>
/// An issued refresh token. Only its SHA-256 hash is stored, so a database leak does not leak
/// usable tokens. A token is used once: refreshing revokes it and records its replacement.
/// </summary>
public class RefreshToken
{
    public int RefreshTokenId { get; set; }
    public int UserId { get; set; }

    /// <summary>Hex SHA-256 of the token.</summary>
    [MaxLength(64), Unicode(false)]
    public string TokenHash { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime? RevokedAt { get; set; }

    /// <summary>Hash of the token issued when this one was used to refresh.</summary>
    [MaxLength(64), Unicode(false)]
    public string? ReplacedByTokenHash { get; set; }

    public User User { get; set; } = null!;
}
