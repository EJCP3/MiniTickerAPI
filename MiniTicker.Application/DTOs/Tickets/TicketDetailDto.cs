using MiniTicker.Core.Application.Catalogs;
using MiniTicker.Core.Application.Comments;
using MiniTicker.Core.Application.Users;
using MiniTicker.Core.Application.Tickets;

namespace MiniTicker.Core.Application.Read
{
    public class TicketDetailDto
    {
        public Guid Id { get; set; }
        public string Numero { get; set; } = null!;
        public string Asunto { get; set; } = null!;
        public string Descripcion { get; set; } = null!;
        public string Estado { get; set; } = null!;
        public string Prioridad { get; set; } = null!;
        public string FechaCreacion { get; set; } = null!;

        // --- AGREGA ESTA PROPIEDAD ---
        public string? FechaActualizacion { get; set; }
        public AreaDto Area { get; set; } = null!;
        public TipoSolicitudDto TipoSolicitud { get; set; } = null!;
        public UserDto Solicitante { get; set; } = null!;
        public UserDto? Gestor { get; set; }
        public string? ArchivoAdjuntoUrl { get; set; }
        public List<ComentarioDto> Comentarios { get; set; } = new();
        public List<TicketHistoryDto> Historial { get; set; } = new();
    }
}