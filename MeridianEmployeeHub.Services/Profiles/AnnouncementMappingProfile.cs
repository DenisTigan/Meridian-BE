using AutoMapper;
using MeridianEmployeeHub.Data.Entities;
using MeridianEmployeeHub.Services.Announcements.DTOs;

namespace MeridianEmployeeHub.Services.Profiles
{
    public class AnnouncementMappingProfile : Profile
    {
        public AnnouncementMappingProfile()
        {
            // Announcement → AnnouncementDto
            // AuthorFullName este aplanat din navigation property Author
            CreateMap<Announcement, AnnouncementDto>()
                .ForMember(
                    dest => dest.AuthorFullName,
                    opt => opt.MapFrom(src => src.Author.FirstName + " " + src.Author.LastName));
        }
    }
}
