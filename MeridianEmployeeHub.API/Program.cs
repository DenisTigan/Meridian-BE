using System.Text;
using MeridianEmployeeHub.API.Middleware;
using MeridianEmployeeHub.Data.Context;
using MeridianEmployeeHub.Data.Repositories;
using MeridianEmployeeHub.Data.Repositories.Interfaces;
using MeridianEmployeeHub.Services.Auth;
using MeridianEmployeeHub.Services.Departments;
using MeridianEmployeeHub.Services.Employees;
using MeridianEmployeeHub.Services.Profiles;
using MeridianEmployeeHub.Services.Roles;
using MeridianEmployeeHub.Services.Teams;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ── 1. Database ──────────────────────────────────────────────────────────────
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// ── 2. AutoMapper ────────────────────────────────────────────────────────────
builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddProfile<EmployeeMappingProfile>();
});

// ── 3. Repositories ──────────────────────────────────────────────────────────
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<IDepartmentRepository, DepartmentRepository>();
builder.Services.AddScoped<ITeamRepository, TeamRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();

// ── 4. Services ──────────────────────────────────────────────────────────────
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IDepartmentService, DepartmentService>();
builder.Services.AddScoped<ITeamService, TeamService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// ── 5. JWT Authentication ────────────────────────────────────────────────────
var jwtSection = builder.Configuration.GetSection("Jwt");
var jwtKey = jwtSection["Key"] ?? throw new InvalidOperationException("JWT Key not configured.");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSection["Issuer"],
        ValidAudience = jwtSection["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ClockSkew = TimeSpan.Zero  // Fara toleranta de timp — token-ul expira exact la timp
    };
});

// ── 6. RBAC Authorization Policies ──────────────────────────────────────────
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("HROrAdmin", p => p.RequireRole("HR", "Admin"));
    options.AddPolicy("ManagerOrAbove", p => p.RequireRole("Manager", "HR", "Admin"));
    options.AddPolicy("AdminOnly", p => p.RequireRole("Admin"));
});

// ── 7. Controllers + Swagger ─────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Meridian Employee Hub API", Version = "v1" });

    // Adauga suport pentru JWT in Swagger UI (butonul Authorize)
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header. Exemplu: Bearer {token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// ── Build ────────────────────────────────────────────────────────────────────
var app = builder.Build();

// ── Pipeline ─────────────────────────────────────────────────────────────────
// ExceptionHandlingMiddleware PRIMUL — intercepteaza toate exceptiile necaptate
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Ordinea obligatorie: Authentication INAINTEA Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
