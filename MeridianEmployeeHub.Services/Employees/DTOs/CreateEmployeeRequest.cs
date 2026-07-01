namespace MeridianEmployeeHub.Services.Employees.DTOs
{
    public class CreateEmployeeRequest
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string JobTitle { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}