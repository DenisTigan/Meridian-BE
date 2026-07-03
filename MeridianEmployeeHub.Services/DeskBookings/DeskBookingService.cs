using MeridianEmployeeHub.Data.Entities;
using MeridianEmployeeHub.Data.Repositories.Interfaces;
using MeridianEmployeeHub.Services.DeskBookings.DTOs;
using MeridianEmployeeHub.Services.Exceptions;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;

namespace MeridianEmployeeHub.Services.DeskBookings
{
    public class DeskBookingService : IDeskBookingService
    {
        private readonly IDeskBookingRepository _bookingRepository;
        private readonly IDeskRepository _deskRepository;

        // Mesaj de conflict reutilizat — același text la Strat 1 și Strat 2
        private const string DeskAlreadyBookedMessage =
            "This desk is already booked for the selected date.";

        public DeskBookingService(
            IDeskBookingRepository bookingRepository,
            IDeskRepository deskRepository)
        {
            _bookingRepository = bookingRepository;
            _deskRepository = deskRepository;
        }

        // ── GET rezervările proprii (paginat) ─────────────────────────────────
        public async Task<PagedBookingResponse> GetMyBookingsAsync(
            int employeeId,
            DateOnly? from,
            DateOnly? to,
            int page,
            int pageSize)
        {
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 100);

            var (items, totalCount) = await _bookingRepository.GetByEmployeeAsync(
                employeeId, from, to, page, pageSize);

            return new PagedBookingResponse
            {
                Items = items.Select(ToDto),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        // ── GET rezervările unei date (ManagerOrAbove) ────────────────────────
        public async Task<IEnumerable<DeskBookingDto>> GetBookingsByDateAsync(DateOnly date)
        {
            var bookings = await _bookingRepository.GetByDateAsync(date);
            return bookings.Select(ToDto);
        }

        // ── POST creare rezervare — AMBELE straturi anti-coliziune ────────────
        public async Task<DeskBookingDto> CreateBookingAsync(
            CreateBookingRequest request,
            int employeeId)
        {
            // Validare desk există și e activ
            var desk = await _deskRepository.GetByIdAsync(request.DeskId)
                ?? throw new KeyNotFoundException($"Desk with id {request.DeskId} not found.");

            if (!desk.IsActive)
                throw new InvalidOperationException(
                    $"Desk '{desk.DeskCode}' is not available for booking (inactive).");

            // ── Strat 1: verificare explicită înainte de insert ────────────────
            // Prinde 99%+ din coliziuni; Strat 2 acoperă cazul rar de concurență.
            var alreadyBooked = await _bookingRepository.ExistsConfirmedAsync(
                request.DeskId, request.Date);

            if (alreadyBooked)
                throw new ConflictException(DeskAlreadyBookedMessage);

            // Creare booking — Confirm() setează simultan Status și ConfirmedDeskId
            var booking = new DeskBooking
            {
                DeskId = request.DeskId,
                EmployeeId = employeeId,
                BookingDate = request.Date
            };
            booking.Confirm(); // SINGURUL loc unde Confirm() e apelat la creare

            try
            {
                await _bookingRepository.AddAsync(booking);

                // ── Strat 2: indexul UNIQUE pe (ConfirmedDeskId, BookingDate) ──
                // Dacă doi request-uri simultane au trecut amândouă de Strat 1,
                // SaveChangesAsync() va arunca MySqlException(1062) pentru al doilea.
                await _bookingRepository.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
                when (ex.InnerException is MySqlException { Number: 1062 })
            {
                // ── Traducerea erorii DB → ConflictException ──────────────────
                // Utilizatorul nu vede niciodată o eroare SQL brută.
                // Același mesaj ca Strat 1 — din perspectiva clientului e identic.
                throw new ConflictException(DeskAlreadyBookedMessage, ex);
            }

            // Re-fetch cu Include pentru a popula navigation properties în DTO
            var created = await _bookingRepository.GetByIdAsync(booking.Id)
                ?? throw new InvalidOperationException(
                    "Failed to retrieve the created booking.");

            return ToDto(created);
        }

        // ── DELETE (soft) — anulare rezervare proprie ─────────────────────────
        public async Task CancelBookingAsync(int bookingId, int currentUserId)
        {
            var booking = await _bookingRepository.GetByIdAsync(bookingId)
                ?? throw new KeyNotFoundException($"Booking with id {bookingId} not found.");

            // Ownership check — identic cu Employee/WorkStatus pattern
            if (booking.EmployeeId != currentUserId)
                throw new ForbiddenException(
                    "You can only cancel your own bookings.");

            if (booking.Status == BookingStatus.Cancelled)
                throw new InvalidOperationException(
                    "This booking is already cancelled.");

            // Cancel() setează simultan Status = Cancelled, ConfirmedDeskId = null, CancelledAt
            booking.Cancel(); // SINGURUL loc unde Cancel() e apelat

            await _bookingRepository.UpdateAsync(booking);
            await _bookingRepository.SaveChangesAsync();
        }

        // ── GET disponibilitate deskuri pentru o dată ─────────────────────────
        // Returnează toate deskurile active cu flag IsAvailable per desk.
        // officeId opțional — filtrează la un singur birou dacă specificat.
        public async Task<IEnumerable<DeskAvailabilityDto>> GetDeskAvailabilityAsync(
            int? officeId,
            DateOnly date)
        {
            // ID-urile deskurilor cu rezervare Confirmed — un singur query, O(1) lookup ulterior
            var bookedDeskIds = await _bookingRepository.GetConfirmedDeskIdsForDateAsync(date);

            IEnumerable<Desk> desks;

            if (officeId.HasValue)
            {
                desks = await _deskRepository.GetByOfficeIdAsync(officeId.Value);
            }
            else
            {
                desks = await _deskRepository.GetAllActiveAsync();
            }

            return desks
                .Where(d => d.IsActive)
                .Select(d => new DeskAvailabilityDto
                {
                    Id = d.Id,
                    OfficeId = d.OfficeId,
                    DeskCode = d.DeskCode,
                    Floor = d.Floor,
                    Zone = d.Zone,
                    PositionX = d.PositionX,
                    PositionY = d.PositionY,
                    IsActive = d.IsActive,
                    IsAvailable = !bookedDeskIds.Contains(d.Id)
                });
        }

        // ── GET prezență — cine e în birou la data cerută ─────────────────────
        public async Task<IEnumerable<PresenceDto>> GetPresenceAsync(
            DateOnly date,
            int? departmentId)
        {
            var bookings = await _bookingRepository.GetPresenceAsync(date, departmentId);

            return bookings.Select(b => new PresenceDto
            {
                EmployeeId = b.EmployeeId,
                FullName = b.Employee.FirstName + " " + b.Employee.LastName,
                DepartmentName = b.Employee.Department.Name,
                DeskId = b.DeskId,
                DeskCode = b.Desk.DeskCode,
                OfficeName = b.Desk.Office.Name
            });
        }

        // ── Mapare manuală DeskBooking → DeskBookingDto ───────────────────────
        // ConfirmedDeskId nu apare în DTO — detaliu intern de persistență
        private static DeskBookingDto ToDto(DeskBooking b) => new()
        {
            Id = b.Id,
            DeskId = b.DeskId,
            DeskCode = b.Desk?.DeskCode ?? string.Empty,
            OfficeName = b.Desk?.Office?.Name ?? string.Empty,
            EmployeeId = b.EmployeeId,
            EmployeeFullName = b.Employee is not null
                ? b.Employee.FirstName + " " + b.Employee.LastName
                : string.Empty,
            BookingDate = b.BookingDate,
            Status = b.Status,
            CreatedAt = b.CreatedAt,
            CancelledAt = b.CancelledAt
        };
    }
}
