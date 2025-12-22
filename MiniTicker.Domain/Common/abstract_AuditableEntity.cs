namespace MiniTicker.Core.Domain.Domain
{
    public abstract class AuditableEntity : BaseEntity
    {
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaModificacion { get; set; }
    }
}