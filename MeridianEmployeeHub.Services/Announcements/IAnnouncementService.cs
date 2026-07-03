using MeridianEmployeeHub.Data.Entities;
using MeridianEmployeeHub.Services.Announcements.DTOs;

namespace MeridianEmployeeHub.Services.Announcements
{
    public interface IAnnouncementService
    {
        Task<PagedAnnouncementResponse> GetAllAsync(
            AnnouncementCategory? category,
            bool isPrivileged,
            int currentUserId,
            int page,
            int pageSize);

        // GetById respectă aceeași regulă de vizibilitate:
        // dacă e nepublicat, doar autorul/Admin/HR îl vede
        Task<AnnouncementDto?> GetByIdAsync(int id, bool isPrivileged, int currentUserId);

        Task<AnnouncementDto> CreateAsync(CreateAnnouncementRequest request, int authorId);

        // Ownership check în service: author sau admin
        Task<AnnouncementDto> UpdateAsync(
            int id,
            UpdateAnnouncementRequest request,
            int currentUserId,
            bool isAdmin);

        // DELETE — apelat doar de Admin (verificat prin policy pe controller)
        Task DeleteAsync(int id);
    }
}
