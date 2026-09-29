namespace PRN232.LMS.API.Models;

public record ApiResponse<T>(bool Success, string Message, T? Data, object? Errors)
{
    public static ApiResponse<T> Ok(T data, string message = "Request processed successfully") => new(true, message, data, null);
    public static ApiResponse<T> Fail(string message, object? errors = null) => new(false, message, default, errors);
}

public record CollectionResponse<T>(bool Success, string Message, IReadOnlyList<T> Data, object? Errors, PRN232.LMS.Services.Models.Pagination Pagination)
{
    public static CollectionResponse<object> From(PRN232.LMS.Services.Models.PagedResult result, string message) =>
        new(true, message, result.Items, null, result.Pagination);
}
