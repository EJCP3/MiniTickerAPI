using System;
using MiniTicker.Core.Domain.Enums;

namespace MiniTicker.Core.Domain.Entities
{
    public class TicketEvent
    {
        public Guid Id { get; set; }
        public Guid TicketId { get; set; }
        public Guid UsuarioId { get; set; }

        public TicketEventType TipoEvento { get; set; }
        public string? Texto { get; set; }

        public EstadoTicket? EstadoAnterior { get; set; }
        public EstadoTicket? EstadoNuevo { get; set; }

        public DateTime Fecha { get; set; } = DateTime.UtcNow;

      
        public virtual Ticket Ticket { get; set; } = null!;
        public virtual Usuario Usuario { get; set; } = null!;
    }
}