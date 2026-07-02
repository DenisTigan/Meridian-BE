using MeridianEmployeeHub.Services.Auth.DTOs;

namespace MeridianEmployeeHub.Services.Auth
{
    public interface IAuthService
    {
        // Login: verifica credentialele, returneaza token scurt (IsFirstLogin=true) sau normal
        Task<LoginResponse> LoginAsync(LoginRequest request);

        // Schimbare parola: accepta doar JWT cu requiresPasswordChange=true claim
        // La succes: seteaza IsFirstLogin=false, emite token normal + refresh token
        Task<LoginResponse> ChangePasswordAsync(int employeeId, ChangePasswordRequest request);

        // Refresh: valideaza refresh token-ul si emite un access token nou
        Task<LoginResponse> RefreshTokenAsync(string refreshToken);

        // Logout: invalideaza refresh token-ul din DB (seteaza null)
        Task LogoutAsync(int employeeId);
    }
}
