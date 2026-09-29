namespace PRN232.LMS.Services.Exceptions;

/// <summary>The requested resource does not exist. Returned to the client as 404.</summary>
public class NotFoundException(string message) : Exception(message);
