using AutoMapper;
using MeridianEmployeeHub.Data.Entities;
using MeridianEmployeeHub.Data.Repositories.Interfaces;
using MeridianEmployeeHub.Services.Employees.DTOs;

namespace MeridianEmployeeHub.Services.Employees
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IEmployeeRepository _repository;
        private readonly IMapper _mapper;

        public EmployeeService(IEmployeeRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<EmployeeDto>> GetAllEmployeesAsync()
        {
            var employees = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<EmployeeDto>>(employees); // Transformăm lista de Entities în DTOs
        }

        public async Task<EmployeeDto?> GetEmployeeByIdAsync(int id)
        {
            var employee = await _repository.GetByIdAsync(id);
            return employee == null ? null : _mapper.Map<EmployeeDto>(employee);
        }

        public async Task<EmployeeDto> CreateEmployeeAsync(CreateEmployeeRequest request)
        {
            var employeeEntity = _mapper.Map<Employee>(request);

            // Generăm un cod de angajat (vom face o logică mai complexă mai târziu)
            employeeEntity.EmployeeCode = $"EMP-{Guid.NewGuid().ToString().Substring(0, 4).ToUpper()}";

            // Deocamdată punem parola așa cum vine. BCrypt pentru securitate vom adăuga când facem modulul de Auth!
            employeeEntity.PasswordHash = request.Password;

            await _repository.AddAsync(employeeEntity);
            await _repository.SaveChangesAsync();

            return _mapper.Map<EmployeeDto>(employeeEntity);
        }
    }
}