using MeridianEmployeeHub.Services.Buddy.DTOs;

namespace MeridianEmployeeHub.Services.Buddy
{
    public interface IBuddyService
    {
        // Returnează toate assignment-urile — disponibil doar HR/Admin
        Task<IEnumerable<BuddyAssignmentDto>> GetAllAssignmentsAsync();

        // Returnează assignment-ul ACTIV al angajatului curent ca new hire.
        // Returnează null dacă nu există (stare normală, nu eroare).
        Task<BuddyAssignmentDto?> GetActiveAssignmentForEmployeeAsync(int employeeId);

        // Creează un assignment nou.
        // Aruncă ConflictException dacă NewEmployeeId are deja un assignment activ.
        // Aruncă KeyNotFoundException dacă NewEmployeeId sau BuddyId nu există.
        Task<BuddyAssignmentDto> AssignBuddyAsync(AssignBuddyRequest request);

        // Actualizează buddy-ul și/sau notele unui assignment existent.
        // Aruncă KeyNotFoundException dacă assignment-ul nu există.
        // Aruncă KeyNotFoundException dacă noul BuddyId nu există.
        Task<BuddyAssignmentDto> UpdateAssignmentAsync(int id, UpdateBuddyAssignmentRequest request);

        // Marchează un assignment ca și Completed.
        // Aruncă KeyNotFoundException dacă assignment-ul nu există.
        Task<BuddyAssignmentDto> CompleteAssignmentAsync(int id);
    }
}
