using MeridianEmployeeHub.Services.Desks.DTOs;

namespace MeridianEmployeeHub.Services.Desks
{
    public interface IDeskService
    {
        Task<IEnumerable<DeskDto>> GetDesksByOfficeAsync(int officeId);

        // Toate deskurile active din toate office-urile (fallback când nu e specificat officeId)
        Task<IEnumerable<DeskDto>> GetAllActiveDesksAsync();

        Task<DeskDto?> GetDeskByIdAsync(int id);
        Task<DeskDto> CreateDeskAsync(CreateDeskRequest request);
        Task<DeskDto> UpdateDeskAsync(int id, UpdateDeskRequest request);

        // Soft-delete — setează IsActive = false, nu șterge fizic
        Task DeactivateDeskAsync(int id);
    }
}
