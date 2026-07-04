namespace MeridianEmployeeHub.Data.Entities
{
    // HRTicket nu moștenește BaseEntity — entitate de tranzacție HR cu propriile câmpuri de audit.
    // TicketNumber este generat secvențial în service (HR-0001, HR-0042, etc.) — nu UUID aleatoriu.
    public class HRTicket
    {
        public int Id { get; set; }

        // Format HR-NNNN (secvențial). Max 20 caractere — suportă HR-9999 și mai mult.
        public string TicketNumber { get; set; } = string.Empty;

        // FK NOT NULL → Employees — cine a depus tichetul
        public int EmployeeId { get; set; }
        public Employee Employee { get; set; } = null!;

        // Categoria tichetului
        public TicketCategory Category { get; set; }

        public string Subject { get; set; } = string.Empty;

        // Conținut lung — stocat ca TEXT în DB
        public string Description { get; set; } = string.Empty;

        // Status curent — default Open la creare
        public TicketStatus Status { get; set; } = TicketStatus.Open;

        // FK nullable → Employees (angajat HR/Admin asignat)
        public int? AssignedToId { get; set; }
        public Employee? AssignedTo { get; set; }

        // Setat automat când Status devine Resolved; resetat la null dacă tichetul e redeschis
        public DateTime? ResolvedAt { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
