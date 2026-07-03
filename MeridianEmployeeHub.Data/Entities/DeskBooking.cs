namespace MeridianEmployeeHub.Data.Entities
{
    // DeskBooking nu moștenește BaseEntity — entitate de tranzacție cu propriul CreatedAt.
    //
    // ── Strat 2: prevenire coliziuni la nivel DB ───────────────────────────────
    // MySQL 8 nu suportă indexuri filtrate (partial indexes). Workaround standard:
    // coloana ConfirmedDeskId (nullable) — conține DeskId când Status = Confirmed,
    // NULL când Status = Cancelled. Indexul UNIQUE pe (ConfirmedDeskId, BookingDate)
    // ignoră automat rândurile cu NULL, deci rezervările anulate nu blochează noi
    // rezervări pe același desk-zi.
    //
    // ── Sincronizare Status ↔ ConfirmedDeskId ─────────────────────────────────
    // Logica de sincronizare este encapsulată EXCLUSIV în metodele Confirm() și Cancel()
    // de pe entitate — nu există nicio altă locație unde ConfirmedDeskId este setat.
    public class DeskBooking
    {
        public int Id { get; set; }

        // FK NOT NULL → Desks, Restrict (istoricul rămâne la soft-delete pe desk)
        public int DeskId { get; set; }
        public Desk Desk { get; set; } = null!;

        // FK NOT NULL → Employees, Restrict
        public int EmployeeId { get; set; }
        public Employee Employee { get; set; } = null!;

        public DateOnly BookingDate { get; set; }

        public BookingStatus Status { get; private set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? CancelledAt { get; set; }

        // ── Discriminator pentru indexul UNIQUE la nivel DB ───────────────────
        // Populat cu DeskId când Status = Confirmed, NULL când Status = Cancelled.
        // NU apare în niciun DTO — este detaliu intern de persistență.
        // Accesat prin EF Core; setter public pentru EF Core materialization.
        public int? ConfirmedDeskId { get; private set; }

        // ── Metode de stare — SINGURUL loc unde Status și ConfirmedDeskId se sincronizează
        public void Confirm()
        {
            Status = BookingStatus.Confirmed;
            ConfirmedDeskId = DeskId;
        }

        public void Cancel()
        {
            Status = BookingStatus.Cancelled;
            ConfirmedDeskId = null;
            CancelledAt = DateTime.UtcNow;
        }
    }
}
