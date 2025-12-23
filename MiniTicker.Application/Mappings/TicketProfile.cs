using AutoMapper;
using MiniTicker.Core.Domain.Entities;
using MiniTicker.Core.Application.Tickets;
using MiniTicker.Core.Application.Read;

namespace MiniTicker.Core.Application.Mappings
{
    public class TicketProfile : Profile
    {
        public TicketProfile()
        {
            // Ticket -> TicketDto
            CreateMap<Ticket, TicketDto>()
                .ForMember(d => d.Estado, opt => opt.MapFrom(s => s.Estado.ToString()))
                .ForMember(d => d.Prioridad, opt => opt.MapFrom(s => s.Prioridad.ToString()));

            // Ticket -> TicketDetailDto
            CreateMap<Ticket, TicketDetailDto>()
                .ForMember(d => d.Estado, opt => opt.MapFrom(s => s.Estado.ToString()))
                .ForMember(d => d.Prioridad, opt => opt.MapFrom(s => s.Prioridad.ToString()));

            // CreateTicketDto -> Ticket
            CreateMap<CreateTicketDto, Ticket>()
                .ForMember(d => d.Id, opt => opt.Ignore())
                .ForMember(d => d.ArchivoAdjuntoUrl, opt => opt.Ignore())
                .ForMember(d => d.FechaCreacion, opt => opt.Ignore())
                .ForMember(d => d.FechaModificacion, opt => opt.Ignore())
                .ForMember(d => d.SolicitanteId, opt => opt.Ignore())
                .ForMember(d => d.GestorAsignadoId, opt => opt.Ignore())
                .ForMember(d => d.Estado, opt => opt.Ignore())
                .ForMember(d => d.Numero, opt => opt.Ignore());

            // UpdateTicketDto -> Ticket
            CreateMap<UpdateTicketDto, Ticket>()
                .ForMember(d => d.Id, opt => opt.Ignore())
                .ForMember(d => d.Numero, opt => opt.Ignore())
                .ForMember(d => d.SolicitanteId, opt => opt.Ignore())
                .ForMember(d => d.GestorAsignadoId, opt => opt.Ignore())
                .ForMember(d => d.ArchivoAdjuntoUrl, opt => opt.Ignore())
                .ForMember(d => d.Estado, opt => opt.Ignore())
                .ForMember(d => d.AreaId, opt => opt.Ignore())
                .ForMember(d => d.TipoSolicitudId, opt => opt.Ignore())
                .ForMember(d => d.FechaCreacion, opt => opt.Ignore())
                .ForMember(d => d.FechaModificacion, opt => opt.Ignore());

            // AssignTicketDto -> Ticket
            CreateMap<AssignTicketDto, Ticket>()
                .ForMember(d => d.GestorAsignadoId, opt => opt.MapFrom(s => s.GestorId))
                .ForMember(d => d.Id, opt => opt.Ignore())
                .ForMember(d => d.Numero, opt => opt.Ignore())
                .ForMember(d => d.SolicitanteId, opt => opt.Ignore())
                .ForMember(d => d.ArchivoAdjuntoUrl, opt => opt.Ignore())
                .ForMember(d => d.Estado, opt => opt.Ignore())
                .ForMember(d => d.AreaId, opt => opt.Ignore())
                .ForMember(d => d.TipoSolicitudId, opt => opt.Ignore())
                .ForMember(d => d.Asunto, opt => opt.Ignore())
                .ForMember(d => d.Descripcion, opt => opt.Ignore())
                .ForMember(d => d.Prioridad, opt => opt.Ignore())
                .ForMember(d => d.FechaCreacion, opt => opt.Ignore())
                .ForMember(d => d.FechaModificacion, opt => opt.Ignore())
                .ForMember(d => d.FechaActualizacion, opt => opt.Ignore());

            // ChangeTicketStatusDto -> Ticket
            CreateMap<ChangeTicketStatusDto, Ticket>()
                .ForMember(d => d.Id, opt => opt.Ignore())
                .ForMember(d => d.Numero, opt => opt.Ignore())
                .ForMember(d => d.SolicitanteId, opt => opt.Ignore())
                .ForMember(d => d.GestorAsignadoId, opt => opt.Ignore())
                .ForMember(d => d.ArchivoAdjuntoUrl, opt => opt.Ignore())
                .ForMember(d => d.AreaId, opt => opt.Ignore())
                .ForMember(d => d.TipoSolicitudId, opt => opt.Ignore())
                .ForMember(d => d.Asunto, opt => opt.Ignore())
                .ForMember(d => d.Descripcion, opt => opt.Ignore())
                .ForMember(d => d.Prioridad, opt => opt.Ignore())
                .ForMember(d => d.FechaCreacion, opt => opt.Ignore())
                .ForMember(d => d.FechaModificacion, opt => opt.Ignore())
                .ForMember(d => d.FechaActualizacion, opt => opt.Ignore());
        }
    }
}