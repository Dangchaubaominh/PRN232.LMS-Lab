namespace PRN232.LMS.API.ResponseModels;

/// <summary>
/// Envelope for every response. All four members are always written, even when null.
/// </summary>
public record ApiResponse<T>(bool Success, string Message, T? Data, object? Errors)
{
    public static ApiResponse<T> Ok(T data, string message = "Request processed successfully") => new(true, message, data, null);

    public static ApiResponse<T> Fail(string message, object? errors = null) => new(false, message, default, errors);
}
