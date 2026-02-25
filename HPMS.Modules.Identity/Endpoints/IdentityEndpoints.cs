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

namespace HPMS.Modules.Identity.Endpoints;

public static class IdentityEndpoints
{
    public static void MapIdentityEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/identity").WithTags("Identity");

        // --- 1. Tenant Onboarding ---
        group.MapPost("/tenants", async (string name, IdentityDbContext db) =>
        {
            var newTenant = new Tenant { Name = name };
            db.Tenants.Add(newTenant);
            await db.SaveChangesAsync();
            return Results.Created($"/tenants/{newTenant.Id}", newTenant);
        }).WithName("CreateTenant");

        // --- 2. User Registration ---
        group.MapPost("/users", async (UserRegistrationDto dto, IdentityDbContext db) =>
        {
            var tenantExists = await db.Tenants.AnyAsync(t => t.Id == dto.TenantId);
            if (!tenantExists) return Results.BadRequest("Invalid Tenant ID");

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
        }).WithName("RegisterUser");

        group.MapGet("/users", async (IdentityDbContext db) => await db.Users.ToListAsync())
            .RequireAuthorization()
            .WithName("GetUsers");

        // --- 3. Login & JWT Generation ---
        group.MapPost("/login", async (LoginRequest request, IdentityDbContext db, IConfiguration config) =>
            {
                // Find the user (using IgnoreQueryFilters because we don't have a TenantId yet)
                var user = await db.Users
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(u => u.Username == request.Username);

                // Verify the password using BCrypt
                if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                {
                    return Results.Unauthorized();
                }

                // Create the Claims (key-value pair that describes the user, encoded into the JWT)
                var claims = new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                    new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
                    new Claim("TenantId", user.TenantId.ToString()),
                    new Claim(ClaimTypes.Role, "ClinicAdmin")
                };

                // Generate the Security Key (the stamp that proves the token is real)
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!));

                // Build the Token
                var token = new JwtSecurityToken(
                    issuer: config["Jwt:Issuer"],
                    audience: config["Jwt:Audience"],
                    claims: claims,
                    expires: DateTime.Now.AddHours(8),
                    signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
                );

                return Results.Ok(new { Token = new JwtSecurityTokenHandler().WriteToken(token) });
            })
            .WithName("Login")
            .WithOpenApi();
    }
}