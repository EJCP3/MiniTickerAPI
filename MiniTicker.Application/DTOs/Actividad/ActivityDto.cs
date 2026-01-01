namespace MiniTicker.Core.Application.Actividad
{
    public class ActivityDto
    {
        public Guid Id { get; set; }
        public Guid? TicketId { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string Mensaje { get; set; } = string.Empty;
        public string Fecha { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
    }
}