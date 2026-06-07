using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

using HPMS.Modules.Identity.Authorization;
using HPMS.Modules.Identity.Data;
using HPMS.Modules.Identity.Endpoints;
using HPMS.Modules.Billing.Data;
using HPMS.SharedKernel.Interfaces;
using HPMS.Web.Services;
using HPMS.Scheduling;
using HPMS.Scheduling.Data;
using HPMS.Scheduling.Services;
using HPMS.Modules.Billing;


var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<ITenantProvider, ClaimsTenantProvider>();
// builder.Services.AddScoped<ITenantProvider, FakeTenantProvider>();

// Add DbContexts for Identity, Scheduling, and Billing modules, using the same connection string.
builder.Services.AddDbContext<IdentityDbContext>(options => 
    options.UseSqlServer(connectionString));

builder.Services.AddDbContext<SchedulingDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDbContext<BillingDbContext>(options =>
    options.UseSqlServer(connectionString));

// Register the appointment conflict service for dependency injection.
builder.Services.AddScoped<IAppointmentConflictService, AppointmentConflictService>();

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

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssemblies(
        typeof(Program).Assembly,
        typeof(SchedulingModule).Assembly,
        typeof(BillingModule).Assembly
    );
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AngularClient", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .WithExposedHeaders("X-Tenant-Id"); // If you send it back in headers
    });
});

builder.Services.AddHpmsAuthorization();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "HPMS API", Version = "v1" });

    // 1. Define the "Bearer" security scheme
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter 'Bearer' [space] and then your valid token.\n\nExample: \"Bearer eyJhbG...\""
    });

    // 2. Make sure Swagger uses that scheme globally
    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var identityDb = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
    var schedulingDb = scope.ServiceProvider.GetRequiredService<SchedulingDbContext>();
    var billingDb = scope.ServiceProvider.GetRequiredService<BillingDbContext>();

    identityDb.Database.Migrate();
    schedulingDb.Database.Migrate();
    billingDb.Database.Migrate();
}

// Enable Swagger UI
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication(); 
app.UseAuthorization();
app.UseCors("AngularClient");

app.MapIdentityEndpoints();
app.MapSchedulingEndpoints();
app.MapBillingEndpoints();

app.Run();

public partial class Program { }