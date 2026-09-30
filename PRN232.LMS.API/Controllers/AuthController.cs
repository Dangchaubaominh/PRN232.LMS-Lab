using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using PRN232.LMS.API.Mappings;
using PRN232.LMS.API.RequestModels;
using PRN232.LMS.API.ResponseModels;
using PRN232.LMS.API.Security;
using PRN232.LMS.Services.Services;

namespace PRN232.LMS.API.Controllers;

/// <summary>Login and token lifecycle. Not versioned: the same endpoints serve every API version.</summary>
[ApiController]
[ApiVersionNeutral]
[Route("api/auth")]
public class AuthController(IAuthService authService) : ControllerBase
{
    /// <summary>Exchanges a username and password for an access token and a refresh token.</summary>
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Login([FromBody] LoginRequest request)
    {
        var result = await authService.LoginAsync(request.Username, request.Password);
        return Ok(ApiResponse<AuthResponse>.Ok(result.ToResponse(), "Login successful."));
    }

    /// <summary>Exchanges a refresh token for a new token pair. Each refresh token works once.</summary>
    [AllowAnonymous]
    [HttpPost("refresh-token")]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var result = await authService.RefreshAsync(request.RefreshToken);
        return Ok(ApiResponse<AuthResponse>.Ok(result.ToResponse(), "Token refreshed."));
    }

    /// <summary>Revokes a refresh token. The access token stays valid until it expires.</summary>
    [AllowAnonymous]
    [HttpPost("logout")]
    public async Task<ActionResult<ApiResponse<object>>> Logout([FromBody] RefreshTokenRequest request)
    {
        await authService.LogoutAsync(request.RefreshToken);
        return Ok(new ApiResponse<object>(true, "Logged out.", null, null));
    }

    /// <summary>The user identified by the access token.</summary>
    [Authorize]
    [HttpGet("me")]
    public ActionResult<ApiResponse<CurrentUserResponse>> Me() => Ok(ApiResponse<CurrentUserResponse>.Ok(new CurrentUserResponse
    {
        UserId = int.Parse(User.FindFirst(JwtRegisteredClaimNames.Sub)!.Value),
        Username = User.Identity!.Name!,
        Role = User.FindFirst(JwtTokenService.RoleClaim)!.Value
    }));
}
