using AutoMapper;
using MeridianEmployeeHub.Data.Entities;
using MeridianEmployeeHub.Data.Repositories.Interfaces;
using MeridianEmployeeHub.Services.Departments.DTOs;

namespace MeridianEmployeeHub.Services.Departments
{
    public class DepartmentService : IDepartmentService
    {
        private readonly IDepartmentRepository _repository;
        private readonly IMapper _mapper;

        public DepartmentService(IDepartmentRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<DepartmentDto>> GetAllDepartmentsAsync()
        {
            var departments = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<DepartmentDto>>(departments);
        }

        public async Task<DepartmentDto?> GetDepartmentByIdAsync(int id)
        {
            var department = await _repository.GetByIdAsync(id);
            return department == null ? null : _mapper.Map<DepartmentDto>(department);
        }

        public async Task<DepartmentDto> CreateDepartmentAsync(CreateDepartmentRequest request)
        {
            var department = _mapper.Map<Department>(request);
            await _repository.AddAsync(department);
            await _repository.SaveChangesAsync();
            return _mapper.Map<DepartmentDto>(department);
        }

        public async Task<DepartmentDto> UpdateDepartmentAsync(int id, UpdateDepartmentRequest request)
        {
            var department = await _repository.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"Department with id {id} not found.");

            // Aplicăm doar câmpurile care au valori (patch-like behavior în PUT)
            if (request.Name is not null)
                department.Name = request.Name;

            if (request.Description is not null)
                department.Description = request.Description;

            if (request.HeadEmployeeId.HasValue)
                department.HeadEmployeeId = request.HeadEmployeeId.Value;

            await _repository.UpdateAsync(department);
            await _repository.SaveChangesAsync();

            return _mapper.Map<DepartmentDto>(department);
        }

        public async Task DeleteDepartmentAsync(int id)
        {
            var department = await _repository.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"Department with id {id} not found.");

            // Department nu are soft-delete — ștergere fizică controlată de Admin
            await _repository.DeleteAsync(department);
            await _repository.SaveChangesAsync();
        }
    }
}
