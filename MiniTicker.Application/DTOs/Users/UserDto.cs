using MiniTicker.Core.Domain.Enums;

namespace MiniTicker.Core.Application.Users
{
    public class UserDto
    {
        public Guid Id { get; set; }
        public string Nombre { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Rol { get; set; } = null!;

        public bool Activo { get; set; } = true;
        public string? FotoPerfilUrl { get; set; }

        public DateTime FechaCreacion { get; set; }

        public Guid? AreaId { get; set; } 
    public string? AreaNombre { get; set; } //
        
    }
}