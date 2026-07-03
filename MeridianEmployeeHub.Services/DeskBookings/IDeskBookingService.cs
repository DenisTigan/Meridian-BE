using MeridianEmployeeHub.Services.DeskBookings.DTOs;

namespace MeridianEmployeeHub.Services.DeskBookings
{
    public interface IDeskBookingService
    {
        // GET /bookings — rezervările proprii, paginat
        Task<PagedBookingResponse> GetMyBookingsAsync(
            int employeeId,
            DateOnly? from,
            DateOnly? to,
            int page,
            int pageSize);

        // GET /bookings/date/{date} — toate rezervările unei date (ManagerOrAbove)
        Task<IEnumerable<DeskBookingDto>> GetBookingsByDateAsync(DateOnly date);

        // POST /bookings — creare rezervare (ambele straturi anti-coliziune)
        Task<DeskBookingDto> CreateBookingAsync(CreateBookingRequest request, int employeeId);

        // DELETE /bookings/{id} — anulare proprie (ownership check în service)
        Task CancelBookingAsync(int bookingId, int currentUserId);

        // GET /desks?date= — disponibilitate deskuri pentru o dată
        Task<IEnumerable<DeskAvailabilityDto>> GetDeskAvailabilityAsync(
            int? officeId,
            DateOnly date);

        // GET /desks/presence — cine e în birou azi (sau la data cerută)
        Task<IEnumerable<PresenceDto>> GetPresenceAsync(DateOnly date, int? departmentId);
    }
}
