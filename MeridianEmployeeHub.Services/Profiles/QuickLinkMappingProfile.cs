using AutoMapper;
using MeridianEmployeeHub.Data.Entities;
using MeridianEmployeeHub.Services.QuickLinks.DTOs;

namespace MeridianEmployeeHub.Services.Profiles
{
    public class QuickLinkMappingProfile : Profile
    {
        public QuickLinkMappingProfile()
        {
            // QuickLink → QuickLinkDto — mapare 1:1, fără câmpuri speciale
            CreateMap<QuickLink, QuickLinkDto>();
        }
    }
}
