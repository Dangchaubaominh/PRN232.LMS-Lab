namespace PRN232.LMS.Services.Exceptions;

/// <summary>A sort, fields or expand value names something the resource does not support. Returned as 400.</summary>
public class InvalidQueryException(string message) : Exception(message);
