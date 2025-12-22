using MiniTicker.Core.Domain.Enums;

namespace MiniTicker.Core.Application.Users
{
    public class UserDto
    {
        public Guid Id { get; set; }
        public string Nombre { get; set; } = null!;
        public string Email { get; set; } = null!;
        public Rol Rol { get; set; }
        public string? FotoPerfilUrl { get; set; }
    }
}