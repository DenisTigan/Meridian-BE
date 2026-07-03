using AutoMapper;
using MeridianEmployeeHub.Data.Entities;
using MeridianEmployeeHub.Data.Repositories.Interfaces;
using MeridianEmployeeHub.Services.Buddy.DTOs;
using MeridianEmployeeHub.Services.Exceptions;

namespace MeridianEmployeeHub.Services.Buddy
{
    public class BuddyService : IBuddyService
    {
        private readonly IBuddyRepository _buddyRepository;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IMapper _mapper;

        public BuddyService(
            IBuddyRepository buddyRepository,
            IEmployeeRepository employeeRepository,
            IMapper mapper)
        {
            _buddyRepository = buddyRepository;
            _employeeRepository = employeeRepository;
            _mapper = mapper;
        }

        // ── GET toate assignment-urile (HR/Admin) ─────────────────────────────
        public async Task<IEnumerable<BuddyAssignmentDto>> GetAllAssignmentsAsync()
        {
            var assignments = await _buddyRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<BuddyAssignmentDto>>(assignments);
        }

        // ── GET assignment-ul activ al angajatului curent (self) ──────────────
        // Returnează null dacă nu există — absența e stare normală pentru angajații
        // care nu sunt new hire sau care nu au un buddy asignat încă.
        public async Task<BuddyAssignmentDto?> GetActiveAssignmentForEmployeeAsync(int employeeId)
        {
            var assignment = await _buddyRepository.GetActiveByNewEmployeeIdAsync(employeeId);
            return assignment is null ? null : _mapper.Map<BuddyAssignmentDto>(assignment);
        }

        // ── Creare assignment nou ─────────────────────────────────────────────
        public async Task<BuddyAssignmentDto> AssignBuddyAsync(AssignBuddyRequest request)
        {
            // Regulă de business: NewEmployee poate fi în cel mult un assignment Activ
            var existingActive = await _buddyRepository.GetActiveByNewEmployeeIdAsync(request.NewEmployeeId);
            if (existingActive is not null)
                throw new ConflictException(
                    $"Employee {request.NewEmployeeId} already has an active buddy assignment (id: {existingActive.Id}). " +
                    "Complete or update the existing assignment before creating a new one.");

            // Validare: ambii angajați trebuie să existe
            var newEmployee = await _employeeRepository.GetByIdAsync(request.NewEmployeeId)
                ?? throw new KeyNotFoundException($"Employee with id {request.NewEmployeeId} not found.");

            var buddy = await _employeeRepository.GetByIdAsync(request.BuddyId)
                ?? throw new KeyNotFoundException($"Employee with id {request.BuddyId} not found (buddy).");

            var assignment = new BuddyAssignment
            {
                NewEmployeeId = request.NewEmployeeId,
                BuddyId = request.BuddyId,
                Notes = request.Notes,
                AssignedAt = DateTime.UtcNow,
                Status = BuddyStatus.Active
            };

            await _buddyRepository.AddAsync(assignment);
            await _buddyRepository.SaveChangesAsync();

            // Re-fetch cu Include pentru a popula navigation properties înainte de mapare
            var created = await _buddyRepository.GetByIdAsync(assignment.Id)
                ?? throw new InvalidOperationException("Failed to retrieve the created buddy assignment.");

            return _mapper.Map<BuddyAssignmentDto>(created);
        }

        // ── Actualizare assignment (buddy nou și/sau note) ────────────────────
        public async Task<BuddyAssignmentDto> UpdateAssignmentAsync(int id, UpdateBuddyAssignmentRequest request)
        {
            var assignment = await _buddyRepository.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"Buddy assignment with id {id} not found.");

            if (request.BuddyId.HasValue)
            {
                // Validare: noul buddy trebuie să existe
                _ = await _employeeRepository.GetByIdAsync(request.BuddyId.Value)
                    ?? throw new KeyNotFoundException($"Employee with id {request.BuddyId.Value} not found (buddy).");

                assignment.BuddyId = request.BuddyId.Value;
            }

            if (request.Notes is not null)
                assignment.Notes = request.Notes;

            await _buddyRepository.UpdateAsync(assignment);
            await _buddyRepository.SaveChangesAsync();

            // Re-fetch pentru a reflecta noul BuddyId cu navigation property actualizat
            var updated = await _buddyRepository.GetByIdAsync(assignment.Id)
                ?? throw new InvalidOperationException("Failed to retrieve the updated buddy assignment.");

            return _mapper.Map<BuddyAssignmentDto>(updated);
        }

        // ── Completare assignment ─────────────────────────────────────────────
        public async Task<BuddyAssignmentDto> CompleteAssignmentAsync(int id)
        {
            var assignment = await _buddyRepository.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"Buddy assignment with id {id} not found.");

            assignment.Status = BuddyStatus.Completed;

            await _buddyRepository.UpdateAsync(assignment);
            await _buddyRepository.SaveChangesAsync();

            return _mapper.Map<BuddyAssignmentDto>(assignment);
        }
    }
}
