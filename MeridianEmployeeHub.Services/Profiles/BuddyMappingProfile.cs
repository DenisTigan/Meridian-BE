using AutoMapper;
using MeridianEmployeeHub.Data.Entities;
using MeridianEmployeeHub.Services.Buddy.DTOs;

namespace MeridianEmployeeHub.Services.Profiles
{
    // Profil AutoMapper dedicat modulului Buddy System.
    // Separat de EmployeeMappingProfile pentru a păstra coeziunea pe modul.
    public class BuddyMappingProfile : Profile
    {
        public BuddyMappingProfile()
        {
            // ── BuddyAssignment → BuddyAssignmentDto ──────────────────────────
            // FullName-urile sunt calculate din navigation properties incluse prin Include() în repository.
            // AutoMapper va aplana FirstName + LastName în câmpuri flat pe DTO.
            CreateMap<BuddyAssignment, BuddyAssignmentDto>()
                .ForMember(
                    dest => dest.NewEmployeeFullName,
                    opt => opt.MapFrom(src => src.NewEmployee.FirstName + " " + src.NewEmployee.LastName))
                .ForMember(
                    dest => dest.BuddyFullName,
                    opt => opt.MapFrom(src => src.Buddy.FirstName + " " + src.Buddy.LastName));
        }
    }
}
