using MiniTicker.Core.Domain.Domain;
using MiniTicker.Core.Domain.Enums;

namespace MiniTicker.Core.Domain.Entities
{
    public class Ticket : AuditableEntity
    {
        public string Numero { get; set; } = null!; // Formato: SOL-YYYY-0001 (generado por lógica externa)
        public string Asunto { get; set; } = null!;
        public string Descripcion { get; set; } = null!;
        public Prioridad Prioridad { get; set; }
        public EstadoTicket Estado { get; set; }
        public Guid AreaId { get; set; }
        public Guid TipoSolicitudId { get; set; }
        public Guid SolicitanteId { get; set; }
        public Guid? GestorAsignadoId { get; set; }
        public string? ArchivoAdjuntoUrl { get; set; }
        //public DateTime FechaCreacion { get; set; }
        public DateTime? FechaActualizacion { get; set; }
    }
}