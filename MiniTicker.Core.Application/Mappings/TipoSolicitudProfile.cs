using AutoMapper;
using MiniTicker.Core.Domain.Entities;
using MiniTicker.Core.Application.Catalogs;

namespace MiniTicker.Core.Application.Mappings
{
    public class TipoSolicitudProfile : Profile
    {
        public TipoSolicitudProfile()
        {
            // TipoSolicitud -> TipoSolicitudDto
            CreateMap<TipoSolicitud, TipoSolicitudDto>();
        }
    }
}