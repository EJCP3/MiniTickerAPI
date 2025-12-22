using Microsoft.AspNetCore.Http;
using MiniTicker.Core.Domain.Enums;

namespace MiniTicker.Core.Application.Tickets
{
    public class CreateTicketDto
    {
        public Guid AreaId { get; set; }
        public Guid TipoSolicitudId { get; set; }
        public string Asunto { get; set; } = null!;
        public string Descripcion { get; set; } = null!;
        public Prioridad Prioridad { get; set; }
        public IFormFile? ArchivoAdjunto { get; set; }
    }
}