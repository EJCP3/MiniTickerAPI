using MiniTicker.Core.Application.Catalogs;
using MiniTicker.Core.Application.Users;

namespace MiniTicker.Core.Application.Read
{
    public class TicketDto
    {
        public Guid Id { get; set; }
        public string Numero { get; set; } = null!;
        public string Asunto { get; set; } = null!;
        public string Estado { get; set; } = null!;
        public string Prioridad { get; set; } = null!;
        
        public string? ArchivoAdjuntoUrl { get; set; }
        public UserDto Solicitante { get; set; } = null!;
        public UserDto? Gestor { get; set; }
        public AreaDto Area { get; set; } = null!;
        public TipoSolicitudDto TipoSolicitud { get; set; } = null!;
        public string FechaCreacion { get; set; } = null!;
    }
}