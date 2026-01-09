using Microsoft.EntityFrameworkCore;
using MiniTicker.Core.Domain.Entities;
using MiniTicker.Core.Domain.Enums;
using MiniTicker.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MiniTicker.Infrastructure.Persistence.Seeds
{
    public static class TicketSeed
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            var solicitante = await context.Usuarios.FirstOrDefaultAsync(u => u.Email == "ana@miniticker.com");
            var gestor = await context.Usuarios.FirstOrDefaultAsync(u => u.Email == "juan@miniticker.com");

            var tipoSoporte = await context.TiposSolicitud
                .Include(t => t.Area)
                .FirstOrDefaultAsync(t => t.Nombre == "Soporte PC");

            if (solicitante == null || gestor == null || tipoSoporte == null) return;

            string prefijo = !string.IsNullOrEmpty(tipoSoporte.Area?.Prefijo)
                             ? tipoSoporte.Area.Prefijo
                             : "SOL";

            var numeroTicket = $"{prefijo}-2025-0001";

            var ticket = await context.Tickets.FirstOrDefaultAsync(t => t.Numero == numeroTicket);

            if (ticket == null)
            {
                ticket = new Ticket
                {
                    Id = Guid.NewGuid(),
                    Numero = numeroTicket,
                    Asunto = "Pantalla Azul Error 0x000",
                    Descripcion = "Al iniciar Windows aparece pantalla azul y se reinicia.",
                    Prioridad = Prioridad.Alta,
                    Estado = EstadoTicket.EnProceso,
                    AreaId = tipoSoporte.AreaId,
                    TipoSolicitudId = tipoSoporte.Id,
                    SolicitanteId = solicitante.Id,
                    GestorAsignadoId = gestor.Id,
                    FechaActualizacion = DateTime.UtcNow
                };
                context.Tickets.Add(ticket);

                // Agregamos eventos iniciales
                var eventos = new List<TicketEvent>
                {
                    new TicketEvent { Id = Guid.NewGuid(), TicketId = ticket.Id, UsuarioId = solicitante.Id, TipoEvento = TicketEventType.Creado, Fecha = DateTime.UtcNow.AddHours(-5), EstadoNuevo = EstadoTicket.Nueva, Texto = "Ticket creado" },
                    new TicketEvent { Id = Guid.NewGuid(), TicketId = ticket.Id, UsuarioId = gestor.Id, TipoEvento = TicketEventType.Asignado, Fecha = DateTime.UtcNow.AddHours(-4), EstadoAnterior = EstadoTicket.Nueva, EstadoNuevo = EstadoTicket.EnProceso, Texto = "Tomado por el gestor" }
                };
                context.TicketEvents.AddRange(eventos);

                await context.SaveChangesAsync();
            }

          
            bool tieneComentarios = await context.Comentarios.AnyAsync(c => c.TicketId == ticket.Id);

            if (!tieneComentarios)
            {
                var comentarios = new List<Comentario>
                {
                    new Comentario { Id = Guid.NewGuid(), TicketId = ticket.Id, UsuarioId = gestor.Id, Texto = "Hola Ana, reinicia en modo seguro. ¿Te sale algún código?", Fecha = DateTime.UtcNow.AddHours(-3) },
                    new Comentario { Id = Guid.NewGuid(), TicketId = ticket.Id, UsuarioId = solicitante.Id, Texto = "Hola Juan, el código es CRITICAL_PROCESS_DIED.", Fecha = DateTime.UtcNow.AddHours(-2) },
                    new Comentario { Id = Guid.NewGuid(), TicketId = ticket.Id, UsuarioId = gestor.Id, Texto = "Entendido. Voy para allá.", Fecha = DateTime.UtcNow.AddHours(-1) }
                };

                context.Comentarios.AddRange(comentarios);
                await context.SaveChangesAsync();
            }
        }
    }
}