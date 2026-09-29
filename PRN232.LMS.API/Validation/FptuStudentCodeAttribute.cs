using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace PRN232.LMS.API.Validation;

/// <summary>
/// Custom validation rule for an FPT University student code: a campus letter (H Hà Nội,
/// S Hồ Chí Minh, D Đà Nẵng, C Cần Thơ, Q Quy Nhơn), a program letter (E, S or A) and a 5 or 6 digit
/// number, e.g. SE19886, CE18793, SE193293. Case-insensitive; the service stores it upper-cased.
/// A null value is valid here and left to [Required].
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
public sealed partial class FptuStudentCodeAttribute()
    : ValidationAttribute("{0} must be an FPTU student code: campus letter (H, S, D, C, Q), program letter (E, S, A) and 5-6 digits, e.g. SE19886.")
{
    public override bool IsValid(object? value) =>
        value is null || (value is string code && StudentCodePattern().IsMatch(code.Trim()));

    [GeneratedRegex("^[HSDCQ][ESA][0-9]{5,6}$", RegexOptions.IgnoreCase)]
    private static partial Regex StudentCodePattern();
}
