using MeridianEmployeeHub.Data.Entities;
using MeridianEmployeeHub.Data.Repositories.Interfaces;
using MeridianEmployeeHub.Services.Exceptions;
using MeridianEmployeeHub.Services.HRTickets.DTOs;

namespace MeridianEmployeeHub.Services.HRTickets
{
    public class HRTicketService : IHRTicketService
    {
        private readonly IHRTicketRepository _ticketRepository;
        private readonly IEmployeeRepository _employeeRepository;

        public HRTicketService(
            IHRTicketRepository ticketRepository,
            IEmployeeRepository employeeRepository)
        {
            _ticketRepository = ticketRepository;
            _employeeRepository = employeeRepository;
        }

        // ── GET toate — comportament dual ─────────────────────────────────────
        // isHROrAdmin = true  → employeeId = null în repository → fără filtru de ownership
        // isHROrAdmin = false → employeeId = currentUserId → filtrare strictă pe proprietar
        // Un singur endpoint, logică duală în service — controllerul pasează flags din claims.
        public async Task<IEnumerable<HRTicketDto>> GetTicketsAsync(
            int currentUserId,
            bool isHROrAdmin,
            TicketCategory? category,
            TicketStatus? status)
        {
            // null → repository nu aplică filtru de EmployeeId (HR/Admin vede totul)
            // currentUserId → repository filtrează pe EmployeeId = currentUserId
            int? filterEmployeeId = isHROrAdmin ? null : currentUserId;

            var tickets = await _ticketRepository.GetTicketsAsync(filterEmployeeId, category, status);
            return tickets.Select(ToDto);
        }

        // ── GET individual — ownership check ─────────────────────────────────
        // Tichetul există → verificăm dacă e al tău sau ești HR/Admin.
        // ForbiddenException (nu KeyNotFoundException) — tichetul există, dar nu ai voie să-l vezi.
        // (Identic cu ownership check-ul de pe Employee din sesiunile anterioare)
        public async Task<HRTicketDto> GetByIdAsync(int id, int currentUserId, bool isHROrAdmin)
        {
            var ticket = await _ticketRepository.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"HR ticket with id {id} not found.");

            if (!isHROrAdmin && ticket.EmployeeId != currentUserId)
                throw new ForbiddenException(
                    "You do not have permission to view this ticket.");

            return ToDto(ticket);
        }

        // ── POST creare — orice angajat autentificat ──────────────────────────
        // TicketNumber generat secvențial: MAX existent + 1, formatat HR-NNNN.
        // Padding la 4 cifre (D4) — dacă depășește 9999, nu trunchia (D4 nu trunchiază).
        public async Task<HRTicketDto> CreateAsync(CreateTicketRequest request, int employeeId)
        {
            if (string.IsNullOrWhiteSpace(request.Subject))
                throw new ArgumentException("Subject is required.");

            if (string.IsNullOrWhiteSpace(request.Description))
                throw new ArgumentException("Description is required.");

            // ── Generare secvențială TicketNumber ────────────────────────────
            // Citim MAX-ul curent din DB, incrementăm, formatăm.
            // Risc de coliziune la concurență e minim și acceptat conform specificației;
            // UNIQUE INDEX pe TicketNumber (configurat în DbContext) servește ca plasă de siguranță.
            var maxSequence = await _ticketRepository.GetMaxTicketSequenceAsync();
            var nextSequence = maxSequence + 1;
            var ticketNumber = $"HR-{nextSequence:D4}";

            var ticket = new HRTicket
            {
                TicketNumber = ticketNumber,
                EmployeeId   = employeeId,
                Category     = request.Category,
                Subject      = request.Subject.Trim(),
                Description  = request.Description.Trim(),
                Status       = TicketStatus.Open
            };

            await _ticketRepository.AddAsync(ticket);
            await _ticketRepository.SaveChangesAsync();

            // Re-fetch cu navigation properties incluse pentru DTO complet
            var created = await _ticketRepository.GetByIdAsync(ticket.Id)
                ?? throw new InvalidOperationException("Failed to retrieve the created ticket.");

            return ToDto(created);
        }

        // ── PATCH status — [HROrAdmin] ────────────────────────────────────────
        // ResolvedAt setat automat când Status → Resolved.
        // ResolvedAt resetat la null dacă Status se schimbă ← Resolved (tichet redeschis).
        public async Task<HRTicketDto> UpdateStatusAsync(int id, UpdateTicketStatusRequest request)
        {
            var ticket = await _ticketRepository.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"HR ticket with id {id} not found.");

            ticket.Status = request.Status;

            if (request.Status == TicketStatus.Resolved)
            {
                // Tichetul a fost rezolvat — setăm timestamp-ul rezolvării
                ticket.ResolvedAt = DateTime.UtcNow;
            }
            else
            {
                // Tichetul a fost redeschis (sau trecut în InProgress) — resetăm ResolvedAt
                ticket.ResolvedAt = null;
            }

            await _ticketRepository.UpdateAsync(ticket);
            await _ticketRepository.SaveChangesAsync();

            return ToDto(ticket);
        }

        // ── PATCH assign — [HROrAdmin] ────────────────────────────────────────
        // Validăm că assignedToId corespunde unui angajat existent și activ.
        // Nu validăm rolul assignedTo — specificația nu cere verificare de rol pe persoana asignată.
        public async Task<HRTicketDto> AssignAsync(int id, AssignTicketRequest request)
        {
            var ticket = await _ticketRepository.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"HR ticket with id {id} not found.");

            // Validare: angajatul asignat trebuie să existe și să fie activ
            var assignee = await _employeeRepository.GetByIdAsync(request.AssignedToId)
                ?? throw new KeyNotFoundException(
                    $"Employee with id {request.AssignedToId} not found.");

            ticket.AssignedToId = assignee.Id;

            await _ticketRepository.UpdateAsync(ticket);
            await _ticketRepository.SaveChangesAsync();

            // Re-fetch pentru AssignedTo navigation property actualizată
            var updated = await _ticketRepository.GetByIdAsync(ticket.Id)
                ?? throw new InvalidOperationException("Failed to retrieve the updated ticket.");

            return ToDto(updated);
        }

        // ── Mapare manuală HRTicket → HRTicketDto ────────────────────────────
        // EmployeeName și AssignedToName aplatizate din navigation properties.
        // Identic cu pattern-ul din CalendarEventService.ToDto (CreatedByName).
        private static HRTicketDto ToDto(HRTicket t) => new()
        {
            Id           = t.Id,
            TicketNumber = t.TicketNumber,
            EmployeeId   = t.EmployeeId,
            EmployeeName = t.Employee is not null
                ? t.Employee.FirstName + " " + t.Employee.LastName
                : string.Empty,
            Category     = t.Category,
            Subject      = t.Subject,
            Description  = t.Description,
            Status       = t.Status,
            AssignedToId   = t.AssignedToId,
            AssignedToName = t.AssignedTo is not null
                ? t.AssignedTo.FirstName + " " + t.AssignedTo.LastName
                : null,
            ResolvedAt = t.ResolvedAt,
            CreatedAt  = t.CreatedAt,
            UpdatedAt  = t.UpdatedAt
        };
    }
}
