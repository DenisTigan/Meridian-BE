using AutoMapper;
using MeridianEmployeeHub.Data.Entities;
using MeridianEmployeeHub.Services.Departments.DTOs;
using MeridianEmployeeHub.Services.Employees.DTOs;
using MeridianEmployeeHub.Services.Roles.DTOs;
using MeridianEmployeeHub.Services.Teams.DTOs;

namespace MeridianEmployeeHub.Services.Profiles
{
    public class EmployeeMappingProfile : Profile
    {
        public EmployeeMappingProfile()
        {
            // ── Employee ──────────────────────────────────────────────────────
            // Spunem AutoMapper-ului că are voie să transforme Employee în EmployeeDto și invers
            CreateMap<Employee, EmployeeDto>();
            CreateMap<CreateEmployeeRequest, Employee>();

            // ── Department ────────────────────────────────────────────────────
            CreateMap<Department, DepartmentDto>();
            CreateMap<CreateDepartmentRequest, Department>();

            // ── Team ──────────────────────────────────────────────────────────
            CreateMap<Team, TeamDto>()
                .ForMember(dest => dest.TeamLeadId, opt => opt.MapFrom(src => src.TeamLeadId));
            CreateMap<CreateTeamRequest, Team>();

            // ── Role ──────────────────────────────────────────────────────────
            CreateMap<Role, RoleDto>();
        }
    }
}