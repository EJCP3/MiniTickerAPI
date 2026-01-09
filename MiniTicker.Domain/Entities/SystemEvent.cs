using System;
using MiniTicker.Core.Domain.Enums;

namespace MiniTicker.Core.Domain.Entities
{
    public class SystemEvent
    {
        public Guid Id { get; set; }
        public Guid UsuarioId { get; set; } // El actor (quien hizo la acción)
        public SystemEventType Tipo { get; set; }
        public string Detalles { get; set; } = string.Empty; // Ej: "Creó el área RRHH"
        public DateTime Fecha { get; set; } = DateTime.UtcNow;

        // Relación con el usuario que hizo la acción
        public virtual Usuario Usuario { get; set; } = null!;
    }
}