namespace PRN232.LMS.Services.Exceptions;

/// <summary>The operation conflicts with the current state of the data, e.g. deleting a row others depend on. Returned as 409.</summary>
public class ConflictException(string message) : Exception(message);
