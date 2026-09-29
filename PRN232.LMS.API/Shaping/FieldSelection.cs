using System.Reflection;
using System.Text.Json;
using PRN232.LMS.Services.Exceptions;

namespace PRN232.LMS.API.Shaping;

/// <summary>
/// The "fields" query option for responses of type <typeparamref name="T"/>: keeps only the
/// requested properties of each item. Parse it before querying so a bad field name fails fast.
/// </summary>
public sealed class FieldSelection<T> where T : notnull
{
    private static readonly PropertyInfo[] Properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

    /// <summary>Null means every property is returned.</summary>
    private readonly IReadOnlyList<PropertyInfo>? _selected;

    private FieldSelection(IReadOnlyList<PropertyInfo>? selected) => _selected = selected;

    /// <exception cref="InvalidQueryException">A requested name is not a property of <typeparamref name="T"/>.</exception>
    public static FieldSelection<T> Parse(string? fields)
    {
        var requested = (fields ?? string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        if (requested.Count == 0)
        {
            return new FieldSelection<T>(null);
        }

        var unknown = requested.FirstOrDefault(name => !Properties.Any(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase)));
        if (unknown is not null)
        {
            throw new InvalidQueryException($"Unknown fields value '{unknown}'.");
        }

        return new FieldSelection<T>(Properties.Where(p => requested.Contains(p.Name)).ToList());
    }

    /// <summary>Returns the item itself, or a camelCase dictionary of just the selected properties.</summary>
    public object Apply(T item) =>
        _selected is null
            ? item
            : _selected.ToDictionary(p => JsonNamingPolicy.CamelCase.ConvertName(p.Name), p => p.GetValue(item));
}
