using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using HPMS.Modules.Identity.Data;
using HPMS.Modules.Identity.DTO;
using HPMS.Modules.Identity.Entities;
using HPMS.SharedKernel.Authorization;
using static HPMS.SharedKernel.Authorization.HpmsRoleIds;

namespace HPMS.Modules.Identity.Endpoints;

public static class IdentityEndpoints
{
    public static void MapIdentityEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/identity").WithTags("Identity");

        // --- 1. Tenant Onboarding (System Admin only) ---
        group.MapPost("/tenants", async (string name, IdentityDbContext db) =>
        {
            var newTenant = new Tenant { Name = name };
            db.Tenants.Add(newTenant);
            await db.SaveChangesAsync();
            return Results.Created($"/tenants/{newTenant.Id}", newTenant);
        })
        .RequireAuthorization(HpmsPolicies.SystemAdmin)
        .WithName("CreateTenant");

        // --- 2. User Registration (Clinic Admin or System Admin) ---
        group.MapPost("/users", async (
            UserRegistrationDto dto,
            IdentityDbContext db,
            ClaimsPrincipal caller) =>
        {
            var authorizationError = ValidateUserRegistration(dto, caller);
            if (authorizationError is not null)
                return authorizationError;

            var tenantExists = await db.Tenants.AnyAsync(t => t.Id == dto.TenantId);
            if (!tenantExists) return Results.BadRequest("Invalid Tenant ID");

            var roleExists = await db.Roles.AnyAsync(r => r.Id == dto.RoleId);
            if (!roleExists) return Results.BadRequest("Invalid Role ID");

            var newUser = new User
            {
                Username = dto.Username,
                Email = dto.Email,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                TenantId = dto.TenantId,
                RoleId = dto.RoleId,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
            };

            db.Users.Add(newUser);
            await db.SaveChangesAsync();
            return Results.Ok(new { newUser.Id, newUser.Username });
        })
        .RequireAuthorization(HpmsPolicies.ClinicAdminOrAbove)
        .WithName("RegisterUser");

        group.MapGet("/users", async (int? roleId, IdentityDbContext db) =>
            await db.Users
                .Include(u => u.Role)
                .Where(u => roleId == null || u.RoleId == roleId)
                .Select(u => new UserSummaryDto(
                    u.Id,
                    u.Username,
                    u.FirstName,
                    u.LastName,
                    u.RoleId,
                    u.Role!.Name))
                .ToListAsync())
            .RequireAuthorization(HpmsPolicies.ClinicAdminOrAbove)
            .WithName("GetUsers");

        group.MapGet("/providers", async (IdentityDbContext db) =>
            await db.Users
                .Include(u => u.Role)
                .Where(u => u.RoleId == Provider)
                .Select(u => new UserSummaryDto(
                    u.Id,
                    u.Username,
                    u.FirstName,
                    u.LastName,
                    u.RoleId,
                    u.Role!.Name))
                .ToListAsync())
            .RequireAuthorization(HpmsPolicies.ClinicalStaff)
            .WithName("GetProviders");

        // --- 3. Login & JWT Generation ---
        group.MapPost("/login", async (LoginRequest request, IdentityDbContext db, IConfiguration config) =>
            {
                var user = await db.Users
                    .IgnoreQueryFilters()
                    .Include(u => u.Role)
                    .FirstOrDefaultAsync(u => u.Username == request.Username);

                if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                {
                    return Results.Unauthorized();
                }

                if (user.Role is null)
                {
                    return Results.Problem("User role is not configured.");
                }

                var expirationHours = request.RememberMe ? 740 : 8;
                var expirationDate = DateTime.UtcNow.AddHours(expirationHours);

                var claims = new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                    new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
                    new Claim("TenantId", user.TenantId.ToString()),
                    new Claim(ClaimTypes.Role, user.Role.Name)
                };

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!));

                var token = new JwtSecurityToken(
                    issuer: config["Jwt:Issuer"],
                    audience: config["Jwt:Audience"],
                    claims: claims,
                    expires: expirationDate,
                    signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
                );

                return Results.Ok(new { Token = new JwtSecurityTokenHandler().WriteToken(token) });
            })
            .WithName("Login")
            .WithOpenApi();
    }

    private static IResult? ValidateUserRegistration(UserRegistrationDto dto, ClaimsPrincipal caller)
    {
        var isSystemAdmin = caller.IsInRole(HpmsRoles.SystemAdmin);
        var isClinicAdmin = caller.IsInRole(HpmsRoles.ClinicAdmin);

        if (!isSystemAdmin && !isClinicAdmin)
            return Results.Forbid();

        if (dto.RoleId == 1 && !isSystemAdmin)
            return Results.Forbid();

        if (isClinicAdmin && !isSystemAdmin)
        {
            var tenantClaim = caller.FindFirst("TenantId")?.Value;
            if (!Guid.TryParse(tenantClaim, out var callerTenantId) || callerTenantId != dto.TenantId)
                return Results.Forbid();
        }

        return null;
    }
}
