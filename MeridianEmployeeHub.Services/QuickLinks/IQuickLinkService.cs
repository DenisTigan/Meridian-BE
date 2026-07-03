using MeridianEmployeeHub.Services.QuickLinks.DTOs;

namespace MeridianEmployeeHub.Services.QuickLinks
{
    public interface IQuickLinkService
    {
        // Returnează lista plată sortată Category → OrderIndex (frontend grupează vizual)
        Task<IEnumerable<QuickLinkDto>> GetAllActiveAsync();

        Task<QuickLinkDto?> GetByIdAsync(int id);

        Task<QuickLinkDto> CreateAsync(CreateQuickLinkRequest request);

        Task<QuickLinkDto> UpdateAsync(int id, UpdateQuickLinkRequest request);

        Task DeleteAsync(int id);

        // Reorder batch: setează OrderIndex pentru fiecare ID în ordinea primită
        Task ReorderAsync(ReorderQuickLinksRequest request);
    }
}
