using MeridianEmployeeHub.Data.Context;
using MeridianEmployeeHub.Data.Entities;
using MeridianEmployeeHub.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MeridianEmployeeHub.Data.Repositories
{
    public class OnboardingRepository : IOnboardingRepository
    {
        private readonly ApplicationDbContext _context;

        public OnboardingRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // Include Tasks pentru a putea calcula OverallProgress și returna lista completă
        public async Task<OnboardingChecklist?> GetByEmployeeIdAsync(int employeeId)
        {
            return await _context.OnboardingChecklists
                .Include(c => c.Tasks.OrderBy(t => t.OrderIndex))
                .FirstOrDefaultAsync(c => c.EmployeeId == employeeId);
        }

        // GetTaskByIdAsync include checklist-ul pentru ownership check (task → checklist → employeeId)
        public async Task<OnboardingTask?> GetTaskByIdAsync(int taskId)
        {
            return await _context.OnboardingTasks
                .Include(t => t.Checklist)
                .FirstOrDefaultAsync(t => t.Id == taskId);
        }

        public async Task AddAsync(OnboardingChecklist checklist)
        {
            await _context.OnboardingChecklists.AddAsync(checklist);
        }

        public Task UpdateAsync(OnboardingChecklist checklist)
        {
            _context.OnboardingChecklists.Update(checklist);
            return Task.CompletedTask;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
