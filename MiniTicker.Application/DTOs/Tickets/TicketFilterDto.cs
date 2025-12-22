using MiniTicker.Core.Domain.Enums;


namespace MiniTicker.Core.Application.Filters
{
    public class TicketFilterDto
    {
        public EstadoTicket? Estado { get; set; }
        public Guid? AreaId { get; set; }
        public Prioridad? Prioridad { get; set; }
        public DateTime? FechaDesde { get; set; }
        public DateTime? FechaHasta { get; set; }
        public string? TextoBusqueda { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}