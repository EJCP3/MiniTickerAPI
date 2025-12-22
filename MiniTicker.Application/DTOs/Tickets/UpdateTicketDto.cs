using MiniTicker.Core.Domain.Enums;

namespace MiniTicker.Core.Application.Tickets
{
    public class UpdateTicketDto
    {
        public string Asunto { get; set; } = null!;
        public string Descripcion { get; set; } = null!;
        public Prioridad Prioridad { get; set; }
    }
}