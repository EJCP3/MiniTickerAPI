using MiniTicker.Core.Domain.Domain;
using MiniTicker.Core.Domain.Enums;

namespace MiniTicker.Core.Domain.Entities
{
    public class Usuario : AuditableEntity
    {
        public string Nombre { get; set; } = null!;
        public string Email { get; set; } = null!;

        public string PasswordHash { get; set; } = null!;

        public Rol Rol { get; set; }
        public Guid? AreaId { get; set; }

        public virtual Area? Area { get; set; }
        public bool Activo { get; set; }
        public string? FotoPerfilUrl { get; set; }
        
        public bool DebeCambiarPassword { get; set; } = true;
    }
}