using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ApartmentMaintenance.Application.Common;
using ApartmentMaintenance.Application.DTOs;
using ApartmentMaintenance.Application.Interfaces;
using ApartmentMaintenance.Domain.Entities;

namespace ApartmentMaintenance.Api.Controllers;

public class AuthController : BaseApiController
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditLogService _auditLogService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IApplicationDbContext context,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService,
        ICurrentUserService currentUserService,
        IAuditLogService auditLogService,
        ILogger<AuthController> logger)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
        _currentUserService = currentUserService;
        _auditLogService = auditLogService;
        _logger = logger;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<LoginResponse>>> Login([FromBody] LoginRequest request)
    {
        var normalizedEmail = request.Email.Trim().ToUpperInvariant();
        var user = await _context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.NormalizedEmail == normalizedEmail);

        if (user == null || !_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            _logger.LogWarning("Failed login attempt for email {Email}", request.Email);
            return FailResponse<LoginResponse>("Invalid email or password.", statusCode: 401);
        }

        if (!user.IsActive)
        {
            return FailResponse<LoginResponse>("Your account has been deactivated. Please contact the administrator.", statusCode: 403);
        }

        var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
        var accessToken = _jwtTokenService.GenerateAccessToken(user, roles);
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var refreshToken = _jwtTokenService.GenerateRefreshToken(user.Id, ipAddress);

        _context.RefreshTokens.Add(refreshToken);
        user.LastLoginAtUtc = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        // Set HttpOnly refresh token cookie
        SetRefreshTokenCookie(refreshToken.Token, refreshToken.ExpiresAtUtc);

        await _auditLogService.LogAsync("User", user.Id.ToString(), "Login", null, $"Successful login from {ipAddress}");

        var response = new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token,
            ExpiresAtUtc = DateTime.UtcNow.AddMinutes(60),
            User = new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber,
                IsActive = user.IsActive,
                Roles = roles,
                LastLoginAtUtc = user.LastLoginAtUtc
            }
        };

        return OkResponse(response, "Login successful.");
    }

    [HttpPost("refresh-token")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<LoginResponse>>> RefreshToken([FromBody] RefreshTokenRequest? request)
    {
        var token = request?.RefreshToken ?? Request.Cookies["refreshToken"];

        if (string.IsNullOrEmpty(token))
        {
            return FailResponse<LoginResponse>("Refresh token is required.", statusCode: 400);
        }

        var storedToken = await _context.RefreshTokens
            .Include(rt => rt.User)
                .ThenInclude(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(rt => rt.Token == token);

        if (storedToken == null || !storedToken.IsValid)
        {
            return FailResponse<LoginResponse>("Invalid or expired refresh token.", statusCode: 401);
        }

        // Token rotation: Revoke current and issue new
        storedToken.IsRevoked = true;
        storedToken.RevokedAtUtc = DateTime.UtcNow;

        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var newRefreshToken = _jwtTokenService.GenerateRefreshToken(storedToken.UserId, ipAddress);
        storedToken.ReplacedByToken = newRefreshToken.Token;

        _context.RefreshTokens.Add(newRefreshToken);
        await _context.SaveChangesAsync();

        SetRefreshTokenCookie(newRefreshToken.Token, newRefreshToken.ExpiresAtUtc);

        var roles = storedToken.User.UserRoles.Select(ur => ur.Role.Name).ToList();
        var accessToken = _jwtTokenService.GenerateAccessToken(storedToken.User, roles);

        var response = new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = newRefreshToken.Token,
            ExpiresAtUtc = DateTime.UtcNow.AddMinutes(60),
            User = new UserDto
            {
                Id = storedToken.User.Id,
                Email = storedToken.User.Email,
                FirstName = storedToken.User.FirstName,
                LastName = storedToken.User.LastName,
                FullName = storedToken.User.FullName,
                PhoneNumber = storedToken.User.PhoneNumber,
                IsActive = storedToken.User.IsActive,
                Roles = roles,
                LastLoginAtUtc = storedToken.User.LastLoginAtUtc
            }
        };

        return OkResponse(response, "Token refreshed successfully.");
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<string>>> Logout()
    {
        var token = Request.Cookies["refreshToken"];
        if (!string.IsNullOrEmpty(token))
        {
            var storedToken = await _context.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == token);
            if (storedToken != null)
            {
                storedToken.IsRevoked = true;
                storedToken.RevokedAtUtc = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        Response.Cookies.Delete("refreshToken");

        if (_currentUserService.UserId.HasValue)
        {
            await _auditLogService.LogAsync("User", _currentUserService.UserId.Value.ToString(), "Logout", null, "User logged out.");
        }

        return OkResponse("Logged out successfully.", "User session ended.");
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<UserDto>>> GetCurrentUser()
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
        {
            return FailResponse<UserDto>("Unauthorized", statusCode: 401);
        }

        var user = await _context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == userId.Value);

        if (user == null)
        {
            return FailResponse<UserDto>("User not found.", statusCode: 404);
        }

        var userDto = new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            FullName = user.FullName,
            PhoneNumber = user.PhoneNumber,
            IsActive = user.IsActive,
            Roles = user.UserRoles.Select(ur => ur.Role.Name).ToList(),
            LastLoginAtUtc = user.LastLoginAtUtc
        };

        return OkResponse(userDto);
    }

    [HttpPost("change-password")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<string>>> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
        {
            return FailResponse<string>("Unauthorized", statusCode: 401);
        }

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId.Value);
        if (user == null)
        {
            return FailResponse<string>("User not found.", statusCode: 404);
        }

        if (!_passwordHasher.VerifyPassword(request.CurrentPassword, user.PasswordHash))
        {
            return FailResponse<string>("Incorrect current password.", statusCode: 400);
        }

        if (string.IsNullOrWhiteSpace(request.NewPassword) || request.NewPassword.Length < 6)
        {
            return FailResponse<string>("New password must be at least 6 characters.", statusCode: 400);
        }

        user.PasswordHash = _passwordHasher.HashPassword(request.NewPassword);
        user.UpdatedAtUtc = DateTime.UtcNow;
        user.UpdatedBy = user.Id.ToString();

        await _context.SaveChangesAsync();
        await _auditLogService.LogAsync("User", user.Id.ToString(), "PasswordChange", null, "Password changed successfully.");

        return OkResponse("Password updated successfully.");
    }

    private void SetRefreshTokenCookie(string token, DateTime expires)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Expires = expires,
            SameSite = SameSiteMode.Lax,
            Secure = Request.IsHttps
        };

        Response.Cookies.Append("refreshToken", token, cookieOptions);
    }
}
