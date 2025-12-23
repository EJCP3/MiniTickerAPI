using AutoMapper;
using MiniTicker.Core.Domain.Entities;
using MiniTicker.Core.Application.Catalogs;

namespace MiniTicker.Core.Application.Mappings
{
    public class AreaProfile : Profile
    {
        public AreaProfile()
        {
            // Area -> AreaDto
            CreateMap<Area, AreaDto>();
        }
    }
}