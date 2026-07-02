using System.Security.Claims;
using MeridianEmployeeHub.Services.Auth;
using MeridianEmployeeHub.Services.Auth.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MeridianEmployeeHub.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private const string RefreshTokenCookieName = "meridian_refresh_token";

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        // POST api/v1/auth/login
        // Pasul 2 & 4: Verifica credentialele si emite token scurt sau normal
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
        {
            var result = await _authService.LoginAsync(request);

            // Daca nu necesita schimbarea parolei, pune refresh token in cookie HttpOnly
            if (!result.RequiresPasswordChange && result.RefreshToken is not null)
            {
                SetRefreshTokenCookie(result.RefreshToken);
                // Nu expunem refresh token-ul in body (ramane in cookie)
                result.RefreshToken = null;
            }

            return Ok(result);
        }

        // POST api/v1/auth/change-password
        // Pasul 3: Accepta doar JWT cu requiresPasswordChange=true claim
        [HttpPost("change-password")]
        [Authorize]
        public async Task<ActionResult<LoginResponse>> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            // Verifica ca JWT-ul curent contine requiresPasswordChange=true
            var requiresChange = User.FindFirstValue("requiresPasswordChange");
            if (requiresChange != "true")
                throw new UnauthorizedAccessException("This endpoint requires a first-login token.");

            var employeeId = GetCurrentEmployeeId();
            var result = await _authService.ChangePasswordAsync(employeeId, request);

            // Emite refresh token in cookie HttpOnly
            if (result.RefreshToken is not null)
            {
                SetRefreshTokenCookie(result.RefreshToken);
                result.RefreshToken = null;
            }

            return Ok(result);
        }

        // POST api/v1/auth/refresh
        // Roteste access token-ul folosind refresh token-ul din cookie
        [HttpPost("refresh")]
        [AllowAnonymous]
        public async Task<ActionResult<LoginResponse>> Refresh()
        {
            var refreshToken = Request.Cookies[RefreshTokenCookieName];
            if (string.IsNullOrEmpty(refreshToken))
                throw new UnauthorizedAccessException("Refresh token not found.");

            var result = await _authService.RefreshTokenAsync(refreshToken);

            // Seteaza noul refresh token in cookie
            if (result.RefreshToken is not null)
            {
                SetRefreshTokenCookie(result.RefreshToken);
                result.RefreshToken = null;
            }

            return Ok(result);
        }

        // POST api/v1/auth/logout
        // Invalideaza refresh token-ul curent (sterge din DB si din cookie)
        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            var employeeId = GetCurrentEmployeeId();
            await _authService.LogoutAsync(employeeId);

            // Sterge cookie-ul de pe client
            Response.Cookies.Delete(RefreshTokenCookieName);

            return NoContent();
        }

        // ── Helper methods ────────────────────────────────────────────────────

        private int GetCurrentEmployeeId()
        {
            var subClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
                        ?? User.FindFirstValue("sub");

            if (!int.TryParse(subClaim, out var employeeId))
                throw new UnauthorizedAccessException("Invalid token: missing or invalid subject claim.");

            return employeeId;
        }

        private void SetRefreshTokenCookie(string refreshToken)
        {
            Response.Cookies.Append(RefreshTokenCookieName, refreshToken, new CookieOptions
            {
                HttpOnly = true,       // Inaccesibil din JavaScript
                Secure = true,         // Transmis doar pe HTTPS
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddDays(7)
            });
        }
    }
}
