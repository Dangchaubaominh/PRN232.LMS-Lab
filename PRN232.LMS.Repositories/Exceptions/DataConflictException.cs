using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace PRN232.LMS.Repositories.Exceptions;

/// <summary>
/// A write broke a unique index or a foreign key. The services check these rules before writing,
/// so this normally means another request changed the same data in between (a race).
/// </summary>
public class DataConflictException(string message, Exception innerException) : Exception(message, innerException)
{
    /// <summary>The conflict behind <paramref name="exception"/>, or null when it is another kind of failure.</summary>
    public static DataConflictException? From(DbUpdateException exception) => exception.InnerException switch
    {
        // 2601 / 2627: duplicate key in a unique index / unique constraint.
        SqlException { Number: 2601 or 2627 } => new("A record with the same unique value already exists.", exception),
        // 547: a foreign key (or check) constraint rejected the change.
        SqlException { Number: 547 } => new("The change conflicts with related records.", exception),
        _ => null
    };
}
