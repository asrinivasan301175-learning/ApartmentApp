using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ApartmentMaintenance.Application.Common;
using ApartmentMaintenance.Application.DTOs;
using ApartmentMaintenance.Application.Interfaces;
using ApartmentMaintenance.Domain.Entities;
using ApartmentMaintenance.Domain.Enums;

namespace ApartmentMaintenance.Api.Controllers;

[Authorize(Roles = SystemRoles.SystemAdmin)]
public class UsersController : BaseApiController
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IAuditLogService _auditLogService;

    public UsersController(
        IApplicationDbContext context,
        IPasswordHasher passwordHasher,
        IAuditLogService auditLogService)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _auditLogService = auditLogService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<UserDto>>>> GetUsers(
        [FromQuery] string? search,
        [FromQuery] string? role,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var query = _context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.Trim().ToLower();
            query = query.Where(u => u.Email.ToLower().Contains(searchLower) ||
                                     u.FirstName.ToLower().Contains(searchLower) ||
                                     u.LastName.ToLower().Contains(searchLower));
        }

        if (!string.IsNullOrWhiteSpace(role))
        {
            query = query.Where(u => u.UserRoles.Any(ur => ur.Role.Name == role));
        }

        var totalCount = await query.CountAsync();
        var users = await query
            .OrderByDescending(u => u.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new UserDto
            {
                Id = u.Id,
                Email = u.Email,
                FirstName = u.FirstName,
                LastName = u.LastName,
                FullName = u.FullName,
                PhoneNumber = u.PhoneNumber,
                IsActive = u.IsActive,
                Roles = u.UserRoles.Select(ur => ur.Role.Name).ToList(),
                LastLoginAtUtc = u.LastLoginAtUtc
            })
            .ToListAsync();

        return OkResponse(new PagedResult<UserDto>(users, totalCount, page, pageSize));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<UserDto>>> CreateUser([FromBody] RegisterUserRequest request)
    {
        var normalizedEmail = request.Email.Trim().ToUpperInvariant();
        if (await _context.Users.AnyAsync(u => u.NormalizedEmail == normalizedEmail))
        {
            return FailResponse<UserDto>("A user with this email address already exists.");
        }

        var user = new User
        {
            Email = request.Email.Trim(),
            NormalizedEmail = normalizedEmail,
            PasswordHash = _passwordHasher.HashPassword(request.Password),
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            PhoneNumber = request.PhoneNumber.Trim(),
            IsActive = true
        };

        _context.Users.Add(user);

        if (request.Roles != null && request.Roles.Any())
        {
            var roles = await _context.Roles.Where(r => request.Roles.Contains(r.Name)).ToListAsync();
            foreach (var r in roles)
            {
                user.UserRoles.Add(new UserRoleMapping { UserId = user.Id, RoleId = r.Id });
            }
        }

        await _context.SaveChangesAsync();
        await _auditLogService.LogAsync("User", user.Id.ToString(), "CreateUser", null, $"Created user {user.Email}");

        return OkResponse(new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            FullName = user.FullName,
            PhoneNumber = user.PhoneNumber,
            IsActive = user.IsActive,
            Roles = request.Roles ?? new()
        }, "User created successfully.");
    }

    [HttpPut("{id}/status")]
    public async Task<ActionResult<ApiResponse<string>>> ToggleUserStatus(Guid id)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user == null)
        {
            return FailResponse<string>("User not found.", statusCode: 404);
        }

        user.IsActive = !user.IsActive;
        await _context.SaveChangesAsync();
        await _auditLogService.LogAsync("User", user.Id.ToString(), "ToggleStatus", null, $"User active status changed to {user.IsActive}");

        return OkResponse($"User status changed to {(user.IsActive ? "Active" : "Inactive")}.");
    }

    [HttpPut("{id}/roles")]
    public async Task<ActionResult<ApiResponse<string>>> AssignRoles(Guid id, [FromBody] AssignRolesRequest request)
    {
        var user = await _context.Users
            .Include(u => u.UserRoles)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
        {
            return FailResponse<string>("User not found.", statusCode: 404);
        }

        var availableRoles = await _context.Roles.Where(r => request.Roles.Contains(r.Name)).ToListAsync();

        user.UserRoles.Clear();
        foreach (var r in availableRoles)
        {
            user.UserRoles.Add(new UserRoleMapping { UserId = user.Id, RoleId = r.Id });
        }

        await _context.SaveChangesAsync();
        await _auditLogService.LogAsync("User", user.Id.ToString(), "AssignRoles", null, $"Assigned roles: {string.Join(", ", request.Roles)}");

        return OkResponse("User roles updated successfully.");
    }
}

[Authorize]
public class RolesController : BaseApiController
{
    private readonly IApplicationDbContext _context;

    public RolesController(IApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<string>>>> GetRoles()
    {
        var roles = await _context.Roles
            .OrderBy(r => r.Id)
            .Select(r => r.Name)
            .ToListAsync();

        return OkResponse(roles);
    }
}
