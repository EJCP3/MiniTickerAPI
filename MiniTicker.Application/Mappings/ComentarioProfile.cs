using AutoMapper;
using MiniTicker.Core.Domain.Entities;
using MiniTicker.Core.Application.Comments;

namespace MiniTicker.Core.Application.Mappings
{
    public class ComentarioProfile : Profile
    {
        public ComentarioProfile()
        {
            // Comentario -> ComentarioDto
            CreateMap<Comentario, ComentarioDto>();

            // CreateComentarioDto -> Comentario
            CreateMap<CreateComentarioDto, Comentario>()
                .ForMember(d => d.Id, opt => opt.Ignore())
                .ForMember(d => d.Fecha, opt => opt.Ignore())
                .ForMember(d => d.UsuarioId, opt => opt.Ignore())
                .ForMember(d => d.TicketId, opt => opt.Ignore());
        }
    }
}