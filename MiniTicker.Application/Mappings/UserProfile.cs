using AutoMapper;
using MiniTicker.Core.Domain.Entities;
using MiniTicker.Core.Application.Users;

namespace MiniTicker.Core.Application.Mappings
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            // Usuario -> UserDto
            CreateMap<Usuario, UserDto>();

            // UpdateUserProfileDto -> Usuario
            CreateMap<UpdateUserProfileDto, Usuario>()
                .ForMember(d => d.Id, opt => opt.Ignore())
                .ForMember(d => d.Rol, opt => opt.Ignore())
                .ForMember(d => d.FotoPerfilUrl, opt => opt.Ignore())
                .ForMember(d => d.FechaCreacion, opt => opt.Ignore())
                .ForMember(d => d.FechaModificacion, opt => opt.Ignore())
                .ForMember(d => d.Email, opt => opt.Ignore())
                .ForMember(d => d.AreaId, opt => opt.Ignore())
                // No se mapea archivo `FotoPerfil` directamente; lo gestiona el servicio de archivos.
                ;
        }
    }
}