Meridian Employee Hub — Backend
A .NET 8 Web API backend for an internal employee hub, covering authentication, employee management, onboarding, desk booking, leave requests, HR ticketing, training, an internal wiki, and more — built with a strict N-Tier architecture and MySQL 8.

Tech Stack
.NET 8 (ASP.NET Core Web API)
MySQL 8 via Entity Framework Core (Pomelo provider)
JWT Bearer authentication with role-based authorization (RBAC)
AutoMapper for entity-to-DTO mapping
FluentValidation for request validation
BCrypt.Net for password hashing
Architecture
The backend follows a layered (N-Tier) architecture across three projects:

MeridianEmployeeHub.API/        → Presentation layer: Controllers, Middleware, Swagger, Program.cs
MeridianEmployeeHub.Services/   → Business logic: Services, DTOs, Validators, AutoMapper profiles
MeridianEmployeeHub.Data/       → Data access: Entities, DbContext, Repositories, Migrations
The dependency rule is strict and enforced throughout: Controllers never access DbContext directly. Every request flows Controller → Service → Repository → DbContext. Business logic (ownership checks, cross-entity rules, RBAC decisions) lives in the Services layer — never in a controller, never in a repository.

Why this matters here specifically
A few conventions were applied consistently across every module, not just as a one-off choice:

Foreign keys to Employee use DeleteBehavior.Restrict, everywhere. Employee uses soft-delete (IsActive = false), never physical deletion. Restrict is a safety net against any accidental hard delete cascading away related history (bookings, tickets, leave records, buddy assignments, etc.). The one deliberate exception is TrainingModule → TrainingCourse, which uses Cascade, because a module has no meaning independent of its parent course — a part-of relationship, not a reference to an independent entity.
Computed values are calculated at read time, not stored and manually kept in sync. Office.TotalDesks, LeaveBalance.RemainingDays, and similar values are derived from already-loaded related data rather than persisted as columns something has to remember to update. This removes an entire class of "stale value" bugs.
Ownership checks live in the Service layer, via exceptions — not via [Authorize] attributes. [Authorize(Policy = "...")] is used for role-based access (HROrAdmin, ManagerOrAbove, AdminOnly). Anything relationship-based ("only the author, or an admin, can edit this") is checked explicitly in the service and throws ForbiddenException (→ 403).
Getting Started
Prerequisites
.NET 8 SDK
MySQL 8 Server running locally (or reachable)
Setup
Clone the repository and restore dependencies:

dotnet restore
Configure your connection string in MeridianEmployeeHub.API/appsettings.json:

"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Port=3306;Database=MeridianHubDb;Uid=root;Pwd=your_password;"
}
Configure JWT settings in the same file, under the Jwt section (key, issuer, audience, and token expiry values). Never commit a real signing key — use dotnet user-secrets or an environment variable for anything beyond local development.

Apply migrations to create the database schema:

dotnet ef database update --project MeridianEmployeeHub.Data --startup-project MeridianEmployeeHub.API
Run the API:

dotnet run --project MeridianEmployeeHub.API
Open Swagger UI (URL printed on startup) to explore and test every endpoint interactively, including JWT bearer authentication.

Creating your first account
There is no self-registration. The very first account must be inserted directly into the database (or seeded), with a BCrypt-hashed password and a valid RoleId pointing to an existing row in Roles. From there, that account (ideally with the Admin role) can create every other employee through POST /api/v1/employees.

Authentication Flow
Login follows a four-step flow, not a single-step one:

Account created by HR/Admin — IsFirstLogin = true, with a temporary password.
First login — POST /api/v1/auth/login detects IsFirstLogin and returns a short-lived (15 min) token carrying a requiresPasswordChange: true claim. No refresh token is issued at this stage.
Forced password change — POST /api/v1/auth/change-password accepts only a token with that claim set. On success, IsFirstLogin is cleared and a full token pair is issued.
Subsequent logins — return a full token pair directly: a 60-minute access token in the response body, plus a 7-day refresh token set as an HttpOnly cookie.
POST /api/v1/auth/refresh and POST /api/v1/auth/logout round out the flow. The access token is designed to live in memory on the client — it is never expected to be persisted to localStorage/sessionStorage.

Roles and authorization
Four fixed roles: Employee, Manager, HR, Admin. Three authorization policies compose them:

Policy	Roles
HROrAdmin	HR, Admin
ManagerOrAbove	Manager, HR, Admin
AdminOnly	Admin
Role itself is treated as fixed configuration data, not a resource with its own CRUD controller — the set of four roles is not expected to change through the API.

Modules
Each module below was built end-to-end (entity → repository → service → DTOs → controller), following the same layered pattern.

Module	Key routes	Notes
Auth	/api/v1/auth/*	See flow above
Employees	/api/v1/employees/*	Self-editable fields vs. HR/Admin-only fields enforced in service; includes profile picture upload and a virtual badge endpoint
Departments / Teams	/api/v1/departments, /api/v1/teams	Standard CRUD, Admin-managed
Onboarding	/api/v1/onboarding/checklist	Checklist is lazily created on first visit; a password_changed task auto-completes via a hook in AuthService.ChangePasswordAsync
Buddy System	/api/v1/buddy/*	One active assignment per new employee, enforced at the service level
Announcements	/api/v1/announcements	Draft visibility limited to the author and HR/Admin
Quick Links	/api/v1/quick-links	Includes a batch /reorder endpoint (single query, no per-item round trips)
Desk Booking	/api/v1/offices, /api/v1/desks, /api/v1/bookings	Double-layer collision prevention — see below
Company Calendar	/api/v1/calendar/events	Range queries use true interval overlap, not just start-date filtering
HR Ticketing	/api/v1/hr/tickets	Single endpoint with role-dependent listing behavior (own tickets vs. all)
Leave Requests / Balance	/api/v1/hr/leave-requests, /api/v1/hr/leave-balance	Approval is a one-way transition; balance sufficiency is checked before mutating any state
Notifications	/api/v1/notifications/*	In-app only; triggered from Leave and HR Ticket services
Training Center	/api/v1/courses, /api/v1/enrollments	New hires are auto-enrolled in mandatory courses on account creation
Internal Wiki	/api/v1/wiki/*	Self-referencing category tree; articles are routed by slug, not numeric ID
A note on Desk Booking's collision prevention
MySQL 8 does not support filtered/partial unique indexes. Preventing two people from booking the same desk on the same day uses two layers instead:

An explicit availability check in DeskBookingService before inserting.
A nullable ConfirmedDeskId column that mirrors DeskId only when a booking's status is Confirmed (null otherwise) — MySQL naturally excludes NULL values from a unique constraint, so cancelled bookings never block a new one. The column has a private setter; the only way to set it is through Confirm()/Cancel() on the entity, keeping the two fields impossible to desynchronize from outside the entity.
If a race condition slips past the first layer, the database-level unique constraint still rejects the duplicate — that specific MySqlException (error 1062) is caught and translated into a clean ConflictException (409), never a raw database error.

Project Conventions
DTOs, never entities, cross the API boundary. Controllers return DTOs; AutoMapper (or manual mapping, where more control is needed) handles the translation.
Custom exceptions map to RFC 7807 responses via a shared ExceptionHandlingMiddleware: ForbiddenException → 403, ConflictException → 409, KeyNotFoundException → 404, ArgumentException → 400, anything else → 500.
Soft-delete over hard-delete, wherever an entity has related history worth preserving (Employee.IsActive, Desk.IsActive, etc.).
Sequential/generated identifiers (like HRTicket.TicketNumber, WikiArticle.Slug) are always generated server-side — never accepted as client input.
Known Limitations (MVP scope)
These were deliberately left out of the current scope, not overlooked:

No certificate PDF generation for completed training (GET .../certificate returns an honest placeholder response, not a broken endpoint).
No interactive desk map rendering (PositionX/PositionY already exist on Desk for when this is built).
No email notifications — in-app only.
Wiki search is a simple LIKE query, not MySQL FULLTEXT indexing.
Profile pictures are stored on local disk, which does not scale across multiple server instances.
See WHAT_I_WOULD_DO_NEXT.md for a prioritized list of what would be built next.

Related Documents
ASSUMPTIONS.md — assumptions made about users, data, and context
DECISIONS.md — product, technical, and UX decisions, and the reasoning behind them
WHAT_I_WOULD_DO_NEXT.md — prioritized roadmap for additional development time
REFLECTION.md — a reflection on the development process itself
