using MiniTicker.Core.Domain.Enums;

namespace MiniTicker.Core.Application.Tickets
{
    public class ChangeTicketStatusDto
    {
        public EstadoTicket Estado { get; set; }
        public string? Motivo { get; set; } // Requerido por validación si Estado == Rechazada
    }
}