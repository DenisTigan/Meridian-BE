using Microsoft.AspNetCore.Http;

namespace MeridianEmployeeHub.Services.Employees
{
    public interface IProfilePictureService
    {
        Task<string> UploadProfilePictureAsync(int employeeId, IFormFile file);
        Task DeleteProfilePictureAsync(int employeeId);
    }
}
