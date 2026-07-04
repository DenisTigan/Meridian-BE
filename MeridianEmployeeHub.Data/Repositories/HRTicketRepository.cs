using MeridianEmployeeHub.Data.Context;
using MeridianEmployeeHub.Data.Entities;
using MeridianEmployeeHub.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MeridianEmployeeHub.Data.Repositories
{
    public class HRTicketRepository : IHRTicketRepository
    {
        private readonly ApplicationDbContext _context;

        public HRTicketRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // ── Generare secvențială TicketNumber ────────────────────────────────────
        // Extrage MAX-ul din partea numerică a TicketNumber-ului existent.
        // Pattern TicketNumber: "HR-NNNN" → substrăm de la indexul 3 și parsăm ca int.
        // Dacă tabelul e gol sau nu există niciun tichet valid, returnăm 0.
        // Service-ul va face: sequence + 1 → format "HR-{n:D4}".
        public async Task<int> GetMaxTicketSequenceAsync()
        {
            // Materializam toate TicketNumber-urile existente în memorie, parsăm partea numerică
            // și luăm MAX-ul. Alternativă: interogare SQL raw, dar aceasta e mai sigură
            // cross-provider și tabloul de tichete nu are volum critic.
            var numbers = await _context.HRTickets
                .Select(t => t.TicketNumber)
                .ToListAsync();

            if (!numbers.Any())
                return 0;

            var max = 0;
            foreach (var number in numbers)
            {
                // Format așteptat: "HR-NNNN" — indexul 3 încoace e partea numerică
                if (number.Length > 3 && int.TryParse(number[3..], out var parsed))
                    max = Math.Max(max, parsed);
            }

            return max;
        }

        // ── Filtrare duală ───────────────────────────────────────────────────────
        // employeeId != null → WHERE EmployeeId = employeeId (angajat obișnuit)
        // employeeId == null → fără filtru de EmployeeId (HR/Admin vede totul)
        public async Task<IEnumerable<HRTicket>> GetTicketsAsync(
            int? employeeId,
            TicketCategory? category,
            TicketStatus? status)
        {
            var query = _context.HRTickets
                .Include(t => t.Employee)
                .Include(t => t.AssignedTo)
                .AsQueryable();

            if (employeeId.HasValue)
                query = query.Where(t => t.EmployeeId == employeeId.Value);

            if (category.HasValue)
                query = query.Where(t => t.Category == category.Value);

            if (status.HasValue)
                query = query.Where(t => t.Status == status.Value);

            return await query
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        // GetByIdAsync include Employee și AssignedTo — necesar pentru ownership check și DTO
        public async Task<HRTicket?> GetByIdAsync(int id)
        {
            return await _context.HRTickets
                .Include(t => t.Employee)
                .Include(t => t.AssignedTo)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task AddAsync(HRTicket ticket)
        {
            await _context.HRTickets.AddAsync(ticket);
        }

        public Task UpdateAsync(HRTicket ticket)
        {
            _context.HRTickets.Update(ticket);
            return Task.CompletedTask;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
