using System;
using MiniTicker.Core.Domain.Enums;

namespace MiniTicker.Core.Application.Tickets
{
    public class TicketHistoryDto
    {
        public string Fecha { get; set; } = null!;
        public TicketEventType TipoEvento { get; set; }

        // Render-friendly
        public string? Titulo { get; set; }     // ej: "Estado actualizado a Cancelado" o "Comentario agregado"
        public string? Subtitulo { get; set; }  // ej: "Por: Administrador"
        public string? Descripcion { get; set; }// ej: "Cambio de estado de En Espera a Cancelado" o el texto del comentario

        // Datos técnicos
        public EstadoTicket? EstadoAnterior { get; set; }
        public EstadoTicket? EstadoNuevo { get; set; }
        public CommentType? TipoComentario { get; set; }
        public bool? VisibleParaSolicitante { get; set; }
        public bool? VisibleSoloGestores { get; set; }
    }
}