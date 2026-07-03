using MeridianEmployeeHub.Data.Entities;
using MeridianEmployeeHub.Data.Repositories.Interfaces;
using MeridianEmployeeHub.Services.Desks.DTOs;

namespace MeridianEmployeeHub.Services.Desks
{
    public class DeskService : IDeskService
    {
        private readonly IDeskRepository _deskRepository;
        private readonly IOfficeRepository _officeRepository;

        // IOfficeRepository injectat pentru validarea existenței office-ului la creare
        public DeskService(IDeskRepository deskRepository, IOfficeRepository officeRepository)
        {
            _deskRepository = deskRepository;
            _officeRepository = officeRepository;
        }

        // ── GET desk-uri per office ────────────────────────────────────────────
        public async Task<IEnumerable<DeskDto>> GetDesksByOfficeAsync(int officeId)
        {
            // Validare: office-ul trebuie să existe
            _ = await _officeRepository.GetByIdAsync(officeId)
                ?? throw new KeyNotFoundException($"Office with id {officeId} not found.");

            var desks = await _deskRepository.GetByOfficeIdAsync(officeId);
            return desks.Select(ToDto);
        }

        // ── GET toate deskurile active din toate office-urile ─────────────────
        public async Task<IEnumerable<DeskDto>> GetAllActiveDesksAsync()
        {
            var desks = await _deskRepository.GetAllActiveAsync();
            return desks.Select(ToDto);
        }

        public async Task<DeskDto?> GetDeskByIdAsync(int id)
        {
            var desk = await _deskRepository.GetByIdAsync(id);
            return desk is null ? null : ToDto(desk);
        }

        // ── POST creare ───────────────────────────────────────────────────────
        public async Task<DeskDto> CreateDeskAsync(CreateDeskRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.DeskCode))
                throw new ArgumentException("DeskCode is required.");

            if (string.IsNullOrWhiteSpace(request.Zone))
                throw new ArgumentException("Zone is required.");

            // Validare: office-ul trebuie să existe
            _ = await _officeRepository.GetByIdAsync(request.OfficeId)
                ?? throw new KeyNotFoundException($"Office with id {request.OfficeId} not found.");

            var desk = new Desk
            {
                OfficeId = request.OfficeId,
                DeskCode = request.DeskCode.Trim(),
                Floor = request.Floor,
                Zone = request.Zone.Trim(),
                PositionX = request.PositionX,
                PositionY = request.PositionY,
                IsActive = request.IsActive
            };

            await _deskRepository.AddAsync(desk);
            await _deskRepository.SaveChangesAsync();

            return ToDto(desk);
        }

        // ── PUT actualizare ───────────────────────────────────────────────────
        public async Task<DeskDto> UpdateDeskAsync(int id, UpdateDeskRequest request)
        {
            var desk = await _deskRepository.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"Desk with id {id} not found.");

            if (request.DeskCode is not null)
                desk.DeskCode = request.DeskCode.Trim();

            if (request.Floor.HasValue)
                desk.Floor = request.Floor.Value;

            if (request.Zone is not null)
                desk.Zone = request.Zone.Trim();

            if (request.PositionX.HasValue)
                desk.PositionX = request.PositionX.Value;

            if (request.PositionY.HasValue)
                desk.PositionY = request.PositionY.Value;

            if (request.IsActive.HasValue)
                desk.IsActive = request.IsActive.Value;

            await _deskRepository.UpdateAsync(desk);
            await _deskRepository.SaveChangesAsync();

            return ToDto(desk);
        }

        // ── DELETE (soft) — setează IsActive = false ──────────────────────────
        // Nu ștergem fizic: sesiunea 8b va lega DeskBookings de Desk prin FK,
        // deci ștergerea fizică ar corupe istoricul de rezervări.
        public async Task DeactivateDeskAsync(int id)
        {
            var desk = await _deskRepository.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"Desk with id {id} not found.");

            desk.IsActive = false;

            await _deskRepository.UpdateAsync(desk);
            await _deskRepository.SaveChangesAsync();
        }

        // ── Mapare manuală Desk → DeskDto ────────────────────────────────────
        private static DeskDto ToDto(Desk desk) => new()
        {
            Id = desk.Id,
            OfficeId = desk.OfficeId,
            DeskCode = desk.DeskCode,
            Floor = desk.Floor,
            Zone = desk.Zone,
            PositionX = desk.PositionX,
            PositionY = desk.PositionY,
            IsActive = desk.IsActive
        };
    }
}
