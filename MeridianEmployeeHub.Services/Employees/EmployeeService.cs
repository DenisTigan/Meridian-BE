using AutoMapper;
using MeridianEmployeeHub.Data.Entities;
using MeridianEmployeeHub.Data.Repositories.Interfaces;
using MeridianEmployeeHub.Services.Employees.DTOs;
using MeridianEmployeeHub.Services.Employees.Validators;
using MeridianEmployeeHub.Services.Exceptions;
using BC = BCrypt.Net.BCrypt;

namespace MeridianEmployeeHub.Services.Employees
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IEmployeeRepository _repository;
        private readonly ILeaveBalanceRepository _leaveBalanceRepository;
        private readonly IMapper _mapper;

        public EmployeeService(
            IEmployeeRepository repository,
            ILeaveBalanceRepository leaveBalanceRepository,
            IMapper mapper)
        {
            _repository = repository;
            _leaveBalanceRepository = leaveBalanceRepository;
            _mapper = mapper;
        }

        // ── Interogare ────────────────────────────────────────────────────────

        public async Task<PagedEmployeeResponse> GetAllEmployeesAsync(
            string? search, int? departmentId, int? teamId, int page, int pageSize)
        {
            // Valori de gardă: limitează pageSize între 1 și 100
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 100);

            var (items, totalCount) = await _repository.GetFilteredAsync(
                search, departmentId, teamId, page, pageSize);

            return new PagedEmployeeResponse
            {
                Items = _mapper.Map<IEnumerable<EmployeeDto>>(items),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<EmployeeDto?> GetEmployeeByIdAsync(int id)
        {
            var employee = await _repository.GetByIdAsync(id);
            return employee == null ? null : _mapper.Map<EmployeeDto>(employee);
        }

        public async Task<EmployeeDto?> GetCurrentUserProfileAsync(int currentUserId)
        {
            // Identic cu GetByIdAsync — ruta /me este sugar syntactic peste același endpoint
            var employee = await _repository.GetByIdAsync(currentUserId);
            return employee == null ? null : _mapper.Map<EmployeeDto>(employee);
        }

        // ── Creare ───────────────────────────────────────────────────────────

        public async Task<EmployeeDto> CreateEmployeeAsync(CreateEmployeeRequest request)
        {
            var employeeEntity = _mapper.Map<Employee>(request);

            // Generam un cod unic de angajat
            employeeEntity.EmployeeCode = $"EMP-{Guid.NewGuid().ToString()[..4].ToUpper()}";

            // Hash BCrypt — parola temporara setata de HR; angajatul o va schimba la primul login
            employeeEntity.PasswordHash = BC.HashPassword(request.Password);

            // Forteaza schimbarea parolei la primul login (securitate implicita)
            employeeEntity.IsFirstLogin = true;

            await _repository.AddAsync(employeeEntity);
            await _repository.SaveChangesAsync(); // We need to save to get the employeeEntity.Id generated

            // Create default Leave Balances for the current year
            short currentYear = (short)DateTime.UtcNow.Year;
            var defaultBalances = new List<LeaveBalance>
            {
                new LeaveBalance { EmployeeId = employeeEntity.Id, Year = currentYear, LeaveType = LeaveType.Annual, AllottedDays = 21, UsedDays = 0 },
                new LeaveBalance { EmployeeId = employeeEntity.Id, Year = currentYear, LeaveType = LeaveType.Sick, AllottedDays = 10, UsedDays = 0 },
                new LeaveBalance { EmployeeId = employeeEntity.Id, Year = currentYear, LeaveType = LeaveType.Personal, AllottedDays = 5, UsedDays = 0 },
                new LeaveBalance { EmployeeId = employeeEntity.Id, Year = currentYear, LeaveType = LeaveType.Maternity, AllottedDays = 90, UsedDays = 0 },
                new LeaveBalance { EmployeeId = employeeEntity.Id, Year = currentYear, LeaveType = LeaveType.Paternity, AllottedDays = 10, UsedDays = 0 }
            };

            await _leaveBalanceRepository.AddRangeAsync(defaultBalances);
            await _leaveBalanceRepository.SaveChangesAsync();

            return _mapper.Map<EmployeeDto>(employeeEntity);
        }

        // ── Actualizare ───────────────────────────────────────────────────────

        public async Task<EmployeeDto> UpdateEmployeeAsync(
            int id, UpdateEmployeeRequest request, int currentUserId, bool isHROrAdmin)
        {
            // Validare input — urmează același pattern ca LoginRequestValidator din AuthService
            var validator = new UpdateEmployeeRequestValidator();
            var validationResult = await validator.ValidateAsync(request);
            if (!validationResult.IsValid)
                throw new ArgumentException(
                    string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage)));

            var employee = await _repository.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"Employee with id {id} not found.");

            // Verificare ownership: un angajat obișnuit poate edita DOAR propriul profil
            if (currentUserId != id && !isHROrAdmin)
                throw new ForbiddenException(
                    "You are not allowed to edit another employee's profile.");

            // ── Câmpuri editabile de orice angajat (self-edit) ────────────────
            if (request.PhoneNumber is not null)
                employee.PhoneNumber = request.PhoneNumber;

            if (request.ProfilePictureUrl is not null)
                employee.ProfilePictureUrl = request.ProfilePictureUrl;

            // ── Câmpuri privilegiate — editabile DOAR de HR/Admin ─────────────
            if (isHROrAdmin)
            {
                if (request.FirstName is not null)
                    employee.FirstName = request.FirstName;

                if (request.LastName is not null)
                    employee.LastName = request.LastName;

                if (request.JobTitle is not null)
                    employee.JobTitle = request.JobTitle;

                if (request.HireDate.HasValue)
                    employee.HireDate = request.HireDate.Value;

                if (request.DepartmentId.HasValue)
                    employee.DepartmentId = request.DepartmentId.Value;

                if (request.TeamId.HasValue)
                    employee.TeamId = request.TeamId.Value;

                if (request.ManagerId.HasValue)
                    employee.ManagerId = request.ManagerId.Value;

                if (request.RoleId.HasValue)
                    employee.RoleId = request.RoleId.Value;
            }

            await _repository.UpdateAsync(employee);
            await _repository.SaveChangesAsync();

            return _mapper.Map<EmployeeDto>(employee);
        }

        public async Task<EmployeeDto> UpdateWorkStatusAsync(
            int id, WorkStatus newStatus, int currentUserId)
        {
            var employee = await _repository.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"Employee with id {id} not found.");

            // Ownership check — un angajat poate schimba DOAR propriul status
            if (currentUserId != id)
                throw new ForbiddenException(
                    "You can only update your own work status.");

            employee.WorkStatus = newStatus;

            await _repository.UpdateAsync(employee);
            await _repository.SaveChangesAsync();

            return _mapper.Map<EmployeeDto>(employee);
        }

        // ── Dezactivare (soft-delete) ─────────────────────────────────────────

        public async Task DeactivateEmployeeAsync(int id)
        {
            var employee = await _repository.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"Employee with id {id} not found.");

            // Soft-delete: setam IsActive = false, NU stergem fizic înregistrarea
            employee.IsActive = false;

            // Invalidam refresh token-ul pentru a forta delogarea imediată
            employee.RefreshToken = null;
            employee.RefreshTokenExpiresAt = null;

            await _repository.UpdateAsync(employee);
            await _repository.SaveChangesAsync();
        }
    }
}