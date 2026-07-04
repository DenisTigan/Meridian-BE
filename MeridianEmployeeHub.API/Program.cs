using System.Text;
using MeridianEmployeeHub.API.Middleware;
using MeridianEmployeeHub.Data.Context;
using MeridianEmployeeHub.Data.Repositories;
using MeridianEmployeeHub.Data.Repositories.Interfaces;
using MeridianEmployeeHub.Services.Announcements;
using MeridianEmployeeHub.Services.Auth;
using MeridianEmployeeHub.Services.Buddy;
using MeridianEmployeeHub.Services.Calendar;
using MeridianEmployeeHub.Services.DeskBookings;
using MeridianEmployeeHub.Services.Departments;
using MeridianEmployeeHub.Services.Desks;
using MeridianEmployeeHub.Services.Employees;
using MeridianEmployeeHub.Services.HRTickets;
using MeridianEmployeeHub.Services.LeaveRequests;
using MeridianEmployeeHub.Services.Notifications;
using MeridianEmployeeHub.Services.Offices;
using MeridianEmployeeHub.Services.Onboarding;
using MeridianEmployeeHub.Services.Profiles;
using MeridianEmployeeHub.Services.Training;
using MeridianEmployeeHub.Services.QuickLinks;
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
    cfg.AddProfile<BuddyMappingProfile>();
    cfg.AddProfile<AnnouncementMappingProfile>();
    cfg.AddProfile<QuickLinkMappingProfile>();
});

// ── 3. Repositories ──────────────────────────────────────────────────────────
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<IDepartmentRepository, DepartmentRepository>();
builder.Services.AddScoped<ITeamRepository, TeamRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IOnboardingRepository, OnboardingRepository>();
builder.Services.AddScoped<IBuddyRepository, BuddyRepository>();
builder.Services.AddScoped<IAnnouncementRepository, AnnouncementRepository>();
builder.Services.AddScoped<IQuickLinkRepository, QuickLinkRepository>();
builder.Services.AddScoped<IOfficeRepository, OfficeRepository>();
builder.Services.AddScoped<IDeskRepository, DeskRepository>();
builder.Services.AddScoped<IDeskBookingRepository, DeskBookingRepository>();
builder.Services.AddScoped<ICalendarEventRepository, CalendarEventRepository>();
builder.Services.AddScoped<IHRTicketRepository, HRTicketRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<ILeaveRequestRepository, LeaveRequestRepository>();
builder.Services.AddScoped<ILeaveBalanceRepository, LeaveBalanceRepository>();
builder.Services.AddScoped<ITrainingCourseRepository, TrainingCourseRepository>();
builder.Services.AddScoped<ICourseEnrollmentRepository, CourseEnrollmentRepository>();

// ── 4. Services ──────────────────────────────────────────────────────────────
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IProfilePictureService, ProfilePictureService>();
builder.Services.AddScoped<IDepartmentService, DepartmentService>();
builder.Services.AddScoped<ITeamService, TeamService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IOnboardingService, OnboardingService>();
builder.Services.AddScoped<IBuddyService, BuddyService>();
builder.Services.AddScoped<IAnnouncementService, AnnouncementService>();
builder.Services.AddScoped<IQuickLinkService, QuickLinkService>();
builder.Services.AddScoped<IOfficeService, OfficeService>();
builder.Services.AddScoped<IDeskService, DeskService>();
builder.Services.AddScoped<IDeskBookingService, DeskBookingService>();
builder.Services.AddScoped<ICalendarEventService, CalendarEventService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IHRTicketService, HRTicketService>();
builder.Services.AddScoped<ILeaveRequestService, LeaveRequestService>();
builder.Services.AddScoped<ILeaveBalanceService, LeaveBalanceService>();
builder.Services.AddScoped<ITrainingCourseService, TrainingCourseService>();
builder.Services.AddScoped<ICourseEnrollmentService, CourseEnrollmentService>();

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

app.UseStaticFiles();
app.UseHttpsRedirection();

// Ordinea obligatorie: Authentication INAINTEA Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
