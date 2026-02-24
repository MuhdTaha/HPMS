using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

using HPMS.Modules.Identity.Data;
using HPMS.Modules.Identity.Entities;
using HPMS.Modules.Identity.DTO;
using HPMS.Modules.Identity.Endpoints;
using HPMS.SharedKernel.Interfaces;
using HPMS.Web.Services;


var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<ITenantProvider, ClaimsTenantProvider>();
// builder.Services.AddScoped<ITenantProvider, FakeTenantProvider>();

builder.Services.AddDbContext<IdentityDbContext>(options => 
    options.UseSqlServer(connectionString));

// Add Authentication "Guard"
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
// Enable Swagger UI
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication(); 
app.UseAuthorization();

app.MapIdentityEndpoints();

app.Run();