using MiniTicker.Core.Domain.Domain;

namespace MiniTicker.Core.Domain.Entities
{
    public class TipoSolicitud : AuditableEntity
    {
        public string Nombre { get; set; } = null!;
        public Guid AreaId { get; set; }
        public bool Activo { get; set; }
    }
}