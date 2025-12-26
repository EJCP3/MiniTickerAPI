using System;
using MiniTicker.Core.Domain.Domain;
using MiniTicker.Core.Domain.Enums;

namespace MiniTicker.Core.Domain.Entities
{
    public class TicketEvent : BaseEntity
    {
        public Guid TicketId { get; set; }
        public Guid UsuarioId { get; set; }
        public TicketEventType TipoEvento { get; set; }

        // Datos comunes
        public DateTime Fecha { get; set; }

        // Para cambios de estado
        public EstadoTicket? EstadoAnterior { get; set; }
        public EstadoTicket? EstadoNuevo { get; set; }

        // Para comentarios
        public CommentType? TipoComentario { get; set; }
        public string? Texto { get; set; }

        // Visibilidad opcional
        public bool? VisibleParaSolicitante { get; set; }
        public bool? VisibleSoloGestores { get; set; }
    }
}