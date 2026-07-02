using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using MeridianEmployeeHub.Data.Entities;
using MeridianEmployeeHub.Data.Repositories.Interfaces;
using MeridianEmployeeHub.Services.Auth.DTOs;
using MeridianEmployeeHub.Services.Auth.Validators;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using BC = BCrypt.Net.BCrypt;

namespace MeridianEmployeeHub.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IConfiguration _configuration;

        // Valori citite din appsettings.json sectiunea "Jwt"
        private readonly string _key;
        private readonly string _issuer;
        private readonly string _audience;
        private readonly int _accessTokenExpiryMinutes;
        private readonly int _shortTokenExpiryMinutes;
        private readonly int _refreshTokenExpiryDays;

        public AuthService(IEmployeeRepository employeeRepository, IConfiguration configuration)
        {
            _employeeRepository = employeeRepository;
            _configuration = configuration;

            var jwt = configuration.GetSection("Jwt");
            _key = jwt["Key"] ?? throw new InvalidOperationException("JWT Key not configured.");
            _issuer = jwt["Issuer"] ?? "meridian-api";
            _audience = jwt["Audience"] ?? "meridian-client";
            _accessTokenExpiryMinutes = int.Parse(jwt["AccessTokenExpiryMinutes"] ?? "60");
            _shortTokenExpiryMinutes = int.Parse(jwt["ShortTokenExpiryMinutes"] ?? "15");
            _refreshTokenExpiryDays = int.Parse(jwt["RefreshTokenExpiryDays"] ?? "7");
        }

        // ── Pasul 2: Login ────────────────────────────────────────────────────
        public async Task<LoginResponse> LoginAsync(LoginRequest request)
        {
            // Validare input
            var validator = new LoginRequestValidator();
            var validationResult = await validator.ValidateAsync(request);
            if (!validationResult.IsValid)
                throw new ArgumentException(string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage)));

            var employee = await _employeeRepository.GetByEmailAsync(request.Email)
                ?? throw new KeyNotFoundException("Invalid email or password.");

            if (!employee.IsActive)
                throw new UnauthorizedAccessException("This account has been deactivated.");

            if (!BC.Verify(request.Password, employee.PasswordHash))
                throw new UnauthorizedAccessException("Invalid email or password.");

            // Pasul 2a: Primul login — JWT scurt (15 min), fara refresh token
            if (employee.IsFirstLogin)
            {
                var shortToken = GenerateAccessToken(employee, requiresPasswordChange: true, _shortTokenExpiryMinutes);
                return new LoginResponse
                {
                    AccessToken = shortToken,
                    RequiresPasswordChange = true,
                    RefreshToken = null
                };
            }

            // Pasul 4: Login standard — JWT normal (60 min) + refresh token (7 zile)
            return await IssueFullTokenPairAsync(employee);
        }

        // ── Pasul 3: Schimbare parola ─────────────────────────────────────────
        public async Task<LoginResponse> ChangePasswordAsync(int employeeId, ChangePasswordRequest request)
        {
            // Validare input
            var validator = new ChangePasswordRequestValidator();
            var validationResult = await validator.ValidateAsync(request);
            if (!validationResult.IsValid)
                throw new ArgumentException(string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage)));

            var employee = await _employeeRepository.GetByIdAsync(employeeId)
                ?? throw new KeyNotFoundException($"Employee with id {employeeId} not found.");

            if (!BC.Verify(request.CurrentPassword, employee.PasswordHash))
                throw new UnauthorizedAccessException("Current password is incorrect.");

            if (request.NewPassword == request.CurrentPassword)
                throw new ArgumentException("New password must be different from the current password.");

            // Actualizeaza parola si reseteaza flag-ul de primul login
            employee.PasswordHash = BC.HashPassword(request.NewPassword);
            employee.IsFirstLogin = false;

            await _employeeRepository.UpdateAsync(employee);
            await _employeeRepository.SaveChangesAsync();

            // Emite token normal + refresh token
            return await IssueFullTokenPairAsync(employee);
        }

        // ── Pasul 4b: Refresh token ───────────────────────────────────────────
        public async Task<LoginResponse> RefreshTokenAsync(string refreshToken)
        {
            var employee = await _employeeRepository.GetByRefreshTokenAsync(refreshToken)
                ?? throw new UnauthorizedAccessException("Invalid or expired refresh token.");

            if (employee.RefreshTokenExpiresAt < DateTime.UtcNow)
            {
                // Curata token-ul expirat
                employee.RefreshToken = null;
                employee.RefreshTokenExpiresAt = null;
                await _employeeRepository.UpdateAsync(employee);
                await _employeeRepository.SaveChangesAsync();
                throw new UnauthorizedAccessException("Refresh token has expired. Please log in again.");
            }

            if (!employee.IsActive)
                throw new UnauthorizedAccessException("This account has been deactivated.");

            return await IssueFullTokenPairAsync(employee);
        }

        // ── Logout ────────────────────────────────────────────────────────────
        public async Task LogoutAsync(int employeeId)
        {
            var employee = await _employeeRepository.GetByIdAsync(employeeId)
                ?? throw new KeyNotFoundException($"Employee with id {employeeId} not found.");

            employee.RefreshToken = null;
            employee.RefreshTokenExpiresAt = null;

            await _employeeRepository.UpdateAsync(employee);
            await _employeeRepository.SaveChangesAsync();
        }

        // ── Metode private helper ─────────────────────────────────────────────

        // Emite access token normal + refresh token; stocheaza refresh token in DB
        private async Task<LoginResponse> IssueFullTokenPairAsync(Employee employee)
        {
            var accessToken = GenerateAccessToken(employee, requiresPasswordChange: false, _accessTokenExpiryMinutes);
            var refreshToken = GenerateRefreshToken();

            employee.RefreshToken = refreshToken;
            employee.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(_refreshTokenExpiryDays);

            await _employeeRepository.UpdateAsync(employee);
            await _employeeRepository.SaveChangesAsync();

            return new LoginResponse
            {
                AccessToken = accessToken,
                RequiresPasswordChange = false,
                RefreshToken = refreshToken
            };
        }

        private string GenerateAccessToken(Employee employee, bool requiresPasswordChange, int expiryMinutes)
        {
            var roleName = employee.Role?.Name ?? string.Empty;

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, employee.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, employee.Email),
                new Claim(ClaimTypes.Role, roleName),
                new Claim("departmentId", employee.DepartmentId.ToString()),
                new Claim("requiresPasswordChange", requiresPasswordChange.ToString().ToLower()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_key));
            var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _issuer,
                audience: _audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // Genereaza un refresh token opac (256 biti de randomness criptografic)
        private static string GenerateRefreshToken()
        {
            var randomBytes = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }
    }
}
