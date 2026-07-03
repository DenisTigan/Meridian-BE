using MeridianEmployeeHub.Data.Entities;
using MeridianEmployeeHub.Data.Repositories.Interfaces;
using MeridianEmployeeHub.Services.Offices.DTOs;

namespace MeridianEmployeeHub.Services.Offices
{
    public class OfficeService : IOfficeService
    {
        private readonly IOfficeRepository _repository;

        // NU folosim AutoMapper pentru Office — TotalDesks este calculat în service,
        // nu poate fi mapat direct (nu e proprietate pe entitate).
        // Maparea manuală este explicită și mai clară pentru acest caz.
        public OfficeService(IOfficeRepository repository)
        {
            _repository = repository;
        }

        // ── GET toate ─────────────────────────────────────────────────────────
        public async Task<IEnumerable<OfficeDto>> GetAllOfficesAsync()
        {
            var offices = await _repository.GetAllAsync();
            return offices.Select(ToDto);
        }

        // ── GET individual ────────────────────────────────────────────────────
        public async Task<OfficeDto?> GetOfficeByIdAsync(int id)
        {
            var office = await _repository.GetByIdAsync(id);
            return office is null ? null : ToDto(office);
        }

        // ── POST creare ───────────────────────────────────────────────────────
        public async Task<OfficeDto> CreateOfficeAsync(CreateOfficeRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                throw new ArgumentException("Name is required.");

            if (string.IsNullOrWhiteSpace(request.Address))
                throw new ArgumentException("Address is required.");

            if (request.FloorCount < 1)
                throw new ArgumentException("FloorCount must be at least 1.");

            var office = new Office
            {
                Name = request.Name.Trim(),
                Address = request.Address.Trim(),
                FloorCount = request.FloorCount
            };

            await _repository.AddAsync(office);
            await _repository.SaveChangesAsync();

            // Re-fetch cu Include(Desks) pentru a returna un DTO complet
            var created = await _repository.GetByIdAsync(office.Id)
                ?? throw new InvalidOperationException("Failed to retrieve the created office.");

            return ToDto(created);
        }

        // ── PUT actualizare ───────────────────────────────────────────────────
        public async Task<OfficeDto> UpdateOfficeAsync(int id, UpdateOfficeRequest request)
        {
            var office = await _repository.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"Office with id {id} not found.");

            if (request.Name is not null)
                office.Name = request.Name.Trim();

            if (request.Address is not null)
                office.Address = request.Address.Trim();

            if (request.FloorCount.HasValue)
            {
                if (request.FloorCount.Value < 1)
                    throw new ArgumentException("FloorCount must be at least 1.");

                office.FloorCount = request.FloorCount.Value;
            }

            await _repository.UpdateAsync(office);
            await _repository.SaveChangesAsync();

            return ToDto(office);
        }

        // ── DELETE ────────────────────────────────────────────────────────────
        // Ștergere fizică — Office nu are soft-delete.
        // Restricție prin FK Restrict pe Desk → Admin trebuie să dezactiveze
        // toate desk-urile înainte de a șterge office-ul.
        public async Task DeleteOfficeAsync(int id)
        {
            var office = await _repository.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"Office with id {id} not found.");

            // Validare: nu poți șterge un office care mai are desk-uri active
            var hasActiveDesks = office.Desks.Any(d => d.IsActive);
            if (hasActiveDesks)
                throw new InvalidOperationException(
                    $"Cannot delete office '{office.Name}' because it still has active desks. " +
                    "Deactivate all desks first.");

            await _repository.DeleteAsync(office);
            await _repository.SaveChangesAsync();
        }

        // ── Mapare manuală Office → OfficeDto ─────────────────────────────────
        // TotalDesks = COUNT(Desks WHERE IsActive = true)
        // Desk-urile sunt deja încărcate prin Include() în repository.
        private static OfficeDto ToDto(Office office) => new()
        {
            Id = office.Id,
            Name = office.Name,
            Address = office.Address,
            FloorCount = office.FloorCount,
            TotalDesks = office.Desks.Count(d => d.IsActive),
            CreatedAt = office.CreatedAt
        };
    }
}
