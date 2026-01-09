namespace MiniTicker.Core.Domain.Entities
{
    public class Area
    {
        public Guid Id { get; set; }
        public string Nombre { get; set; } = string.Empty;

        public string Prefijo { get; set; } = string.Empty;

        public bool Activo { get; set; } = true;

        public Guid? ResponsableId { get; set; }
        public virtual Usuario? Responsable { get; set; }
        public virtual ICollection<Ticket> Tickets { get; set; }
    }
}