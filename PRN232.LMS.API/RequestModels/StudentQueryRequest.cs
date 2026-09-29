using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace PRN232.LMS.API.RequestModels;

/// <summary>Query string for GET /api/v2/students. Unlike v1 there is no "expand": v2 returns enrollmentCount instead.</summary>
public class StudentQueryRequest
{
    /// <summary>Case-insensitive keyword matched against student code, full name and email.</summary>
    /// <example>SE1900</example>
    [FromQuery(Name = "search")]
    public string? Search { get; set; }

    /// <summary>Comma separated field list; prefix a field with '-' for descending order.</summary>
    /// <example>studentCode</example>
    [FromQuery(Name = "sort")]
    public string? Sort { get; set; }

    [FromQuery(Name = "page")]
    [Range(1, int.MaxValue)]
    public int Page { get; set; } = 1;

    [FromQuery(Name = "size")]
    [Range(1, 100)]
    public int Size { get; set; } = 10;

    /// <summary>Comma separated list of properties to return.</summary>
    /// <example>studentCode,fullName,enrollmentCount</example>
    [FromQuery(Name = "fields")]
    public string? Fields { get; set; }
}
