namespace PRN232.LMS.Services.BusinessModels;

public record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalItems, int TotalPages);
