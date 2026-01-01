using System;
using MiniTicker.Core.Domain.Enums;

namespace MiniTicker.Core.Application.Tickets
{
    public class TicketHistoryDto
    {
        public string Fecha { get; set; } = null!;
        public TicketEventType TipoEvento { get; set; }

        public string? Titulo { get; set; }     
        public string? Subtitulo { get; set; }  
        public string? Descripcion { get; set; }

        
        public EstadoTicket? EstadoAnterior { get; set; }
        public EstadoTicket? EstadoNuevo { get; set; }
        public CommentType? TipoComentario { get; set; }
        public bool? VisibleParaSolicitante { get; set; }
        public bool? VisibleSoloGestores { get; set; }
    }
}