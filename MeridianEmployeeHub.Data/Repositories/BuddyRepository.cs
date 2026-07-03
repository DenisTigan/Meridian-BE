using MeridianEmployeeHub.Data.Context;
using MeridianEmployeeHub.Data.Entities;
using MeridianEmployeeHub.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MeridianEmployeeHub.Data.Repositories
{
    public class BuddyRepository : IBuddyRepository
    {
        private readonly ApplicationDbContext _context;

        public BuddyRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // Include ambele navigation properties pentru a permite maparea numelor în BuddyAssignmentDto
        public async Task<IEnumerable<BuddyAssignment>> GetAllAsync()
        {
            return await _context.BuddyAssignments
                .Include(a => a.NewEmployee)
                .Include(a => a.Buddy)
                .OrderByDescending(a => a.AssignedAt)
                .ToListAsync();
        }

        public async Task<BuddyAssignment?> GetByIdAsync(int id)
        {
            return await _context.BuddyAssignments
                .Include(a => a.NewEmployee)
                .Include(a => a.Buddy)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        // Caută assignment-ul activ al unui angajat ca new hire.
        // Folosit de:
        //   1. Regula de business: dacă există → ConflictException la creare
        //   2. GET /my-assignment: returnează null dacă nu există (stare normală)
        public async Task<BuddyAssignment?> GetActiveByNewEmployeeIdAsync(int newEmployeeId)
        {
            return await _context.BuddyAssignments
                .Include(a => a.NewEmployee)
                .Include(a => a.Buddy)
                .FirstOrDefaultAsync(a =>
                    a.NewEmployeeId == newEmployeeId &&
                    a.Status == BuddyStatus.Active);
        }

        public async Task AddAsync(BuddyAssignment assignment)
        {
            await _context.BuddyAssignments.AddAsync(assignment);
        }

        public Task UpdateAsync(BuddyAssignment assignment)
        {
            _context.BuddyAssignments.Update(assignment);
            return Task.CompletedTask;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
