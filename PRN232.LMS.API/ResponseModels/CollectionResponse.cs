namespace PRN232.LMS.API.ResponseModels;

/// <summary>Envelope for a page of a collection.</summary>
public record CollectionResponse<T>(bool Success, string Message, IReadOnlyList<T> Data, object? Errors, PaginationResponse Pagination);

public record PaginationResponse(int Page, int PageSize, int TotalItems, int TotalPages);
