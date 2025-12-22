using MiniTicker.Core.Application.Catalogs;

namespace MiniTicker.Core.Application.Read
{
    public class TicketDto
    {
        public Guid Id { get; set; }
        public string Numero { get; set; } = null!;
        public string Asunto { get; set; } = null!;
        public string Estado { get; set; } = null!;
        public string Prioridad { get; set; } = null!;
        public AreaDto Area { get; set; } = null!;
        public TipoSolicitudDto TipoSolicitud { get; set; } = null!;
        public DateTime FechaCreacion { get; set; }
    }
}