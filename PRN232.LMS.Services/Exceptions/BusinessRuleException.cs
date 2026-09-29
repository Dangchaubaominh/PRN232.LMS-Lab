namespace PRN232.LMS.Services.Exceptions;

/// <summary>The request is well-formed but breaks a business rule. Returned to the client as 400.</summary>
public class BusinessRuleException(string message) : Exception(message);
