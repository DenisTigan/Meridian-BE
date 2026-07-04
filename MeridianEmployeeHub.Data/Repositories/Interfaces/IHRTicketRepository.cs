using MeridianEmployeeHub.Data.Entities;

namespace MeridianEmployeeHub.Data.Repositories.Interfaces
{
    public interface IHRTicketRepository
    {
        // Generare TicketNumber secvențial — returnează numărul maxim curent (partea numerică),
        // sau 0 dacă nu există niciun tichet. Service-ul va incrementa și formata HR-NNNN.
        Task<int> GetMaxTicketSequenceAsync();

        // Comportament dual: angajat obișnuit → filtrare pe employeeId,
        // HR/Admin → toate tichetele (employeeId = null ⇒ fără filtru).
        Task<IEnumerable<HRTicket>> GetTicketsAsync(
            int? employeeId,
            TicketCategory? category,
            TicketStatus? status);

        Task<HRTicket?> GetByIdAsync(int id);

        Task AddAsync(HRTicket ticket);
        Task UpdateAsync(HRTicket ticket);
        Task SaveChangesAsync();
    }
}
