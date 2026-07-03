namespace MeridianEmployeeHub.Services.Buddy.DTOs
{
    // Body pentru PUT /api/v1/buddy/assignments/{id}
    // Permite actualizarea buddy-ului asignat și/sau a notelor.
    // Câmpurile null sunt ignorate (patch-like behavior în PUT, consistent cu restul proiectului).
    public class UpdateBuddyAssignmentRequest
    {
        // Dacă specificat, înlocuiește buddy-ul curent cu un alt angajat
        public int? BuddyId { get; set; }

        // Dacă specificat, actualizează notele assignment-ului
        public string? Notes { get; set; }
    }
}
