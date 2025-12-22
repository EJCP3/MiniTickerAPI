
using MiniTicker.Core.Domain.Domain;

namespace MiniTicker.Core.Domain.Entities
{
    public class Comentario : BaseEntity
    {
        public Guid TicketId { get; set; }
        public Guid UsuarioId { get; set; }
        public string Texto { get; set; } = null!;
        public DateTime Fecha { get; set; }
    }
}