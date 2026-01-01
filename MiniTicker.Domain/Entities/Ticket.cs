using MiniTicker.Core.Domain.Domain;
using MiniTicker.Core.Domain.Enums;

namespace MiniTicker.Core.Domain.Entities
{
    public class Ticket : AuditableEntity
    {
        public string Numero { get; set; } = string.Empty;
        public string Asunto { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;

        public DateTime? FechaActualizacion { get; set; }
        public Prioridad Prioridad { get; set; }
        public EstadoTicket Estado { get; set; }

        // Claves foráneas (IDs)
        public Guid AreaId { get; set; }
        public Guid TipoSolicitudId { get; set; }
        public Guid SolicitanteId { get; set; }
        public Guid? GestorAsignadoId { get; set; }

        public string? ArchivoAdjuntoUrl { get; set; }

      
        public virtual Area Area { get; set; } = null!;
        public virtual TipoSolicitud TipoSolicitud { get; set; } = null!;
        public virtual Usuario Solicitante { get; set; } = null!;
        public virtual Usuario? GestorAsignado { get; set; }
    }
}