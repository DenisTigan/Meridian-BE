namespace MeridianEmployeeHub.Services.Auth.DTOs
{
    public class LoginResponse
    {
        public string AccessToken { get; set; } = string.Empty;

        // true = JWT scurt de 15 min, nu contine RefreshToken
        // false = JWT normal de 60 min + RefreshToken in cookie HttpOnly
        public bool RequiresPasswordChange { get; set; }

        // null cand RequiresPasswordChange = true (refresh token nu se emite la primul login)
        public string? RefreshToken { get; set; }
    }
}
