
using MiniTicker.Core.Domain.Domain;

namespace MiniTicker.Core.Domain.Entities
{
    public class Area : AuditableEntity
    {
        public string Nombre { get; set; } = null!;
        public bool Activo { get; set; }
    }
}