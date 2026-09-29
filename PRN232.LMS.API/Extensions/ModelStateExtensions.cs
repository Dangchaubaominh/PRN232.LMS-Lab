using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace PRN232.LMS.API.Extensions;

public static class ModelStateExtensions
{
    /// <summary>Field name to error messages, for the "errors" member of a validation failure.</summary>
    public static Dictionary<string, string[]> ToErrorDictionary(this ModelStateDictionary modelState) =>
        modelState
            .Where(entry => entry.Value?.Errors.Count > 0)
            .ToDictionary(entry => entry.Key, entry => entry.Value!.Errors.Select(e => e.ErrorMessage).ToArray());
}
