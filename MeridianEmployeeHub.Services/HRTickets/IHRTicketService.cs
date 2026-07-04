using MeridianEmployeeHub.Data.Entities;
using MeridianEmployeeHub.Services.HRTickets.DTOs;

namespace MeridianEmployeeHub.Services.HRTickets
{
    public interface IHRTicketService
    {
        // GET — comportament dual în service:
        //   isHROrAdmin = true  → toate tichetele (fără filtru de ownership)
        //   isHROrAdmin = false → doar tichetele proprii (WHERE EmployeeId = currentUserId)
        Task<IEnumerable<HRTicketDto>> GetTicketsAsync(
            int currentUserId,
            bool isHROrAdmin,
            TicketCategory? category,
            TicketStatus? status);

        // GET individual — ownership check în service: ForbiddenException dacă nu e al tău și nu ești HR/Admin
        Task<HRTicketDto> GetByIdAsync(int id, int currentUserId, bool isHROrAdmin);

        // POST — orice angajat autentificat poate depune
        Task<HRTicketDto> CreateAsync(CreateTicketRequest request, int employeeId);

        // PATCH status — [HROrAdmin]; ResolvedAt setat/resetat automat în funcție de noul status
        Task<HRTicketDto> UpdateStatusAsync(int id, UpdateTicketStatusRequest request);

        // PATCH assign — [HROrAdmin]; validare că assignedToId e un angajat existent și activ
        Task<HRTicketDto> AssignAsync(int id, AssignTicketRequest request);
    }
}
