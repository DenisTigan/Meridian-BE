using MeridianEmployeeHub.Services.Offices.DTOs;

namespace MeridianEmployeeHub.Services.Offices
{
    public interface IOfficeService
    {
        Task<IEnumerable<OfficeDto>> GetAllOfficesAsync();
        Task<OfficeDto?> GetOfficeByIdAsync(int id);
        Task<OfficeDto> CreateOfficeAsync(CreateOfficeRequest request);
        Task<OfficeDto> UpdateOfficeAsync(int id, UpdateOfficeRequest request);
        Task DeleteOfficeAsync(int id);
    }
}
