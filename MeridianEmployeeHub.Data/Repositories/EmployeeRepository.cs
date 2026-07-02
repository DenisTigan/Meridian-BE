using MeridianEmployeeHub.Data.Context;
using MeridianEmployeeHub.Data.Entities;
using MeridianEmployeeHub.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MeridianEmployeeHub.Data.Repositories
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly ApplicationDbContext _context;

        // Injectăm "bucătarul" (DbContext) în "ospătar" (Repository)
        public EmployeeRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Employee>> GetAllAsync()
        {
            // Filtrul IsActive este aplicat automat prin global query filter din OnModelCreating
            return await _context.Employees.ToListAsync();
        }

        public async Task<Employee?> GetByIdAsync(int id)
        {
            // Global query filter exclude automat angajații inactivi
            return await _context.Employees.FirstOrDefaultAsync(e => e.Id == id);
        }

        // IgnoreQueryFilters: permite autentificarea si pentru conturi inactive,
        // astfel incat sa putem returna eroarea corecta ("cont inactiv" vs "email negasit")
        public async Task<Employee?> GetByEmailAsync(string email)
        {
            return await _context.Employees
                .IgnoreQueryFilters()
                .Include(e => e.Role)
                .FirstOrDefaultAsync(e => e.Email == email);
        }

        public async Task<Employee?> GetByRefreshTokenAsync(string refreshToken)
        {
            return await _context.Employees
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(e => e.RefreshToken == refreshToken);
        }

        public async Task AddAsync(Employee employee)
        {
            await _context.Employees.AddAsync(employee);
        }

        public Task UpdateAsync(Employee employee)
        {
            _context.Employees.Update(employee);
            return Task.CompletedTask;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}