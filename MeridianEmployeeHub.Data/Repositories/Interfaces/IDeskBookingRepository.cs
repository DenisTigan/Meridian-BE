using MeridianEmployeeHub.Data.Entities;

namespace MeridianEmployeeHub.Data.Repositories.Interfaces
{
    public interface IDeskBookingRepository
    {
        // Verificare Strat 1: există deja o rezervare Confirmed pe desk-ul dat pentru data dată?
        Task<bool> ExistsConfirmedAsync(int deskId, DateOnly date);

        // Rezervările unui angajat, filtrate opțional pe interval de date, paginat
        Task<(IEnumerable<DeskBooking> Items, int TotalCount)> GetByEmployeeAsync(
            int employeeId,
            DateOnly? from,
            DateOnly? to,
            int page,
            int pageSize);

        // Toate rezervările dintr-o dată (ManagerOrAbove) — cu Employee+Department inclus
        Task<IEnumerable<DeskBooking>> GetByDateAsync(DateOnly date);

        // Angajații prezenți (Confirmed) într-o dată, filtrat opțional pe departament
        Task<IEnumerable<DeskBooking>> GetPresenceAsync(DateOnly date, int? departmentId);

        // ID-urile deskurilor rezervate Confirmed pentru o dată — folosit la disponibilitate
        Task<HashSet<int>> GetConfirmedDeskIdsForDateAsync(DateOnly date);

        Task<DeskBooking?> GetByIdAsync(int id);

        Task AddAsync(DeskBooking booking);
        Task UpdateAsync(DeskBooking booking);
        Task SaveChangesAsync();
    }
}
