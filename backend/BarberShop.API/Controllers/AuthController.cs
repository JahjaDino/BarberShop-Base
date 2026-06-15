using System.Security.Claims;
using BarberShop.API.DTOs.Auth;
using BarberShop.API.Services.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace BarberShop.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [EnableRateLimiting(AuthRateLimitPolicies.Register)]
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _authService.RegisterAsync(request, cancellationToken);
            return CreatedAtAction(nameof(Me), response.User, response);
        }
        catch (InvalidOperationException)
        {
            return Conflict(new { message = "Email is already registered." });
        }
    }

    [EnableRateLimiting(AuthRateLimitPolicies.Login)]
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.LoginAsync(request, cancellationToken);
        if (result.IsLocked)
        {
            return Unauthorized(new { message = "Account is temporarily locked. Please try again later." });
        }

        if (!result.Succeeded)
        {
            return Unauthorized(new { message = "Invalid email or password." });
        }

        return Ok(result.AuthResponse);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var response = await _authService.RefreshAsync(request, cancellationToken);
        if (response is null)
        {
            return Unauthorized(new { message = "Invalid refresh token." });
        }

        return Ok(response);
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout(LogoutRequest request, CancellationToken cancellationToken)
    {
        await _authService.LogoutAsync(request, cancellationToken);
        return Ok(new { message = "Logged out successfully." });
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me(CancellationToken cancellationToken)
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdValue, out var userId))
        {
            return Unauthorized(new { message = "Invalid token." });
        }

        var response = await _authService.GetCurrentUserAsync(userId, cancellationToken);
        if (response is null)
        {
            return Unauthorized(new { message = "Invalid token." });
        }

        return Ok(response);
    }

    [EnableRateLimiting(AuthRateLimitPolicies.ForgotPassword)]
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest request, CancellationToken cancellationToken)
    {
        await _authService.RequestPasswordResetAsync(request, cancellationToken);
        return Ok(new { message = "If an account with this email exists, password reset instructions have been sent." });
    }

    [EnableRateLimiting(AuthRateLimitPolicies.ResetPassword)]
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(ResetPasswordRequest request, CancellationToken cancellationToken)
    {
        var reset = await _authService.ResetPasswordAsync(request, cancellationToken);
        if (!reset)
        {
            return BadRequest(new { message = "Invalid or expired reset token." });
        }

        return Ok(new { message = "Password has been reset successfully." });
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdValue, out var userId))
        {
            return Unauthorized(new { message = "Invalid token." });
        }

        var changed = await _authService.ChangePasswordAsync(userId, request, cancellationToken);
        if (!changed)
        {
            return BadRequest(new { message = "Current password is not valid." });
        }

        return Ok(new { message = "Password has been changed successfully." });
    }
}
