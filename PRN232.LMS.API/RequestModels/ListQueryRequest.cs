using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace PRN232.LMS.API.RequestModels;

/// <summary>
/// Query string contract for every collection endpoint.
///
/// Each property declares its wire name in camelCase with [FromQuery(Name = "...")], so Swagger
/// documents "search" instead of "Search". Model binding itself stays untouched and remains
/// case-insensitive, so "Search" and "search" both still bind.
/// </summary>
public class ListQueryRequest
{
    /// <summary>Case-insensitive keyword matched against the main text fields of the resource.</summary>
    /// <example>nguyen</example>
    [FromQuery(Name = "search")]
    public string? Search { get; set; }

    /// <summary>Filters enrollments by status.</summary>
    /// <example>Active</example>
    [FromQuery(Name = "status")]
    public string? Status { get; set; }

    /// <summary>Filters courses by semester.</summary>
    [FromQuery(Name = "semesterId")]
    public int? SemesterId { get; set; }

    /// <summary>Filters courses by subject.</summary>
    [FromQuery(Name = "subjectId")]
    public int? SubjectId { get; set; }

    /// <summary>Filters enrollments by student.</summary>
    [FromQuery(Name = "studentId")]
    public int? StudentId { get; set; }

    /// <summary>Filters enrollments by course.</summary>
    [FromQuery(Name = "courseId")]
    public int? CourseId { get; set; }

    /// <summary>Comma separated field list; prefix a field with '-' for descending order.</summary>
    /// <example>fullName,-dateOfBirth</example>
    [FromQuery(Name = "sort")]
    public string? Sort { get; set; }

    /// <summary>1-based page number. Defaults to 1.</summary>
    /// <example>1</example>
    [FromQuery(Name = "page")]
    [Range(1, int.MaxValue)]
    public int Page { get; set; } = 1;

    /// <summary>Page size. Defaults to 10, maximum 100.</summary>
    /// <example>10</example>
    [FromQuery(Name = "size")]
    [Range(1, 100)]
    public int Size { get; set; } = 10;

    /// <summary>Comma separated list of properties to return; everything else is omitted.</summary>
    /// <example>studentId,fullName</example>
    [FromQuery(Name = "fields")]
    public string? Fields { get; set; }

    /// <summary>Comma separated list of related resources to include in the response.</summary>
    /// <example>student,course</example>
    [FromQuery(Name = "expand")]
    public string? Expand { get; set; }
}
