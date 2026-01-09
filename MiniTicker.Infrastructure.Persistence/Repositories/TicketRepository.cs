using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MiniTicker.Infrastructure.Persistence;
using MiniTicker.Core.Application.Interfaces.Repositories;
using MiniTicker.Core.Domain.Entities;
using MiniTicker.Core.Application.Filters;

namespace MiniTicker.Infrastructure.Persistence.Repositories
{
    internal class TicketRepository : ITicketRepository
    {
        private readonly ApplicationDbContext _context;

        public TicketRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task AddAsync(Ticket ticket)
        {
            if (ticket == null) throw new ArgumentNullException(nameof(ticket));

            await _context.Tickets.AddAsync(ticket).ConfigureAwait(false);
            await _context.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task UpdateAsync(Ticket ticket)
        {
            if (ticket == null) throw new ArgumentNullException(nameof(ticket));

            _context.Tickets.Update(ticket);
            await _context.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task DeleteAsync(Ticket ticket)
        {
            if (ticket == null) throw new ArgumentNullException(nameof(ticket));

            _context.Tickets.Remove(ticket);
            await _context.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task<Ticket?> GetByIdAsync(Guid ticketId)
        {
            return await _context.Tickets
                .AsNoTracking()
        // 👇 Ahora usamos los nombres que acabamos de crear en la Entidad
        .Include(t => t.TicketEvents)
        .Include(t => t.Comentarios)

        // Incluimos datos extra para que se vea bonito en el front
        .Include(t => t.Solicitante)
        .Include(t => t.GestorAsignado)
                .FirstOrDefaultAsync(t => t.Id == ticketId)
                .ConfigureAwait(false);
        }

        public async Task<int> GetNextSequenceForYearAsync(int year)
        {
            var prefix = $"SOL-{year}-";

            var lastNumero = await _context.Tickets
                .AsNoTracking()
                .Where(t => t.Numero.StartsWith(prefix))
                .OrderByDescending(t => t.Numero)
                .Select(t => t.Numero)
                .FirstOrDefaultAsync()
                .ConfigureAwait(false);

            if (lastNumero == null)
                return 1;

            var sequencePart = lastNumero.Substring(prefix.Length);

            return int.TryParse(sequencePart, out var lastSequence)
                ? lastSequence + 1
                : 1;
        }

        public async Task<Ticket?> GetByNumeroAsync(string numero)
        {
            if (string.IsNullOrWhiteSpace(numero)) throw new ArgumentException("El número es obligatorio.", nameof(numero));

            return await _context.Tickets
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Numero == numero)
                .ConfigureAwait(false);
        }

        public async Task<bool> ExistsAsync(Guid ticketId)
        {
            return await _context.Tickets
                .AsNoTracking()
                .AnyAsync(t => t.Id == ticketId)
                .ConfigureAwait(false);
        }

        public async Task<IReadOnlyList<Ticket>> GetAllAsync()
        {
            var list = await _context.Tickets
                .AsNoTracking()
                .OrderByDescending(t => t.FechaCreacion)
                .ToListAsync()
                .ConfigureAwait(false);

            return list;
        }

        public async Task<bool> AnyAsync(Expression<Func<Ticket, bool>> predicate, CancellationToken cancellationToken = default)
        {
            // Usamos la función AnyAsync de Entity Framework para consultar eficiente
            return await _context.Tickets.AnyAsync(predicate, cancellationToken);
        }

        public async Task<(IReadOnlyList<Ticket> Tickets, int TotalCount)> GetPagedAsync(TicketFilterDto filter)
        {
            if (filter == null) throw new ArgumentNullException(nameof(filter));

            var query = _context.Tickets
          .AsNoTracking()
          .Include(t => t.Area)
          .Include(t => t.Solicitante)
          .Include(t => t.GestorAsignado) // Importante para saber si tiene gestor
          .AsQueryable();

            if (filter.UsuarioId.HasValue)
            {
                // Filtramos para que solo devuelva los tickets creados por este usuario
                query = query.Where(t => t.SolicitanteId == filter.UsuarioId.Value);
            }


            // Aplicar filtros opcionales
            if (filter.Estado.HasValue)
            {
                query = query.Where(t => t.Estado == filter.Estado.Value);
            }

            if (filter.AreaId.HasValue)
            {
                query = query.Where(t => t.AreaId == filter.AreaId.Value);
            }

            if (filter.Prioridad.HasValue)
            {
                query = query.Where(t => t.Prioridad == filter.Prioridad.Value);
            }

            if (filter.FechaDesde.HasValue)
            {
                query = query.Where(t => t.FechaCreacion >= filter.FechaDesde.Value);
            }

            if (filter.FechaHasta.HasValue)
            {
                var fechaFin = filter.FechaHasta.Value.Date.AddDays(1);

                query = query.Where(t => t.FechaCreacion < fechaFin);
            }

            if (filter.TieneGestor.HasValue)
            {
                if (filter.TieneGestor.Value)
                    query = query.Where(t => t.GestorAsignadoId != null); // Tickets asignados
                else
                    query = query.Where(t => t.GestorAsignadoId == null); // Tickets huérfanos
            }

            if (!string.IsNullOrWhiteSpace(filter.TextoBusqueda))
            {
                var texto = filter.TextoBusqueda.Trim().ToLower();
                query = query.Where(t =>
                    (t.Numero != null && t.Numero.ToLower().Contains(texto)) ||
                    (t.Asunto != null && t.Asunto.ToLower().Contains(texto)) ||
                    (t.Descripcion != null && t.Descripcion.ToLower().Contains(texto)));
            }

            // Ordenar por FechaCreacion descendente
            query = query.OrderByDescending(t => t.FechaCreacion);

            // Total antes de paginar
            var totalCount = await query.CountAsync().ConfigureAwait(false);

            // Paginación segura
            var page = filter.Page <= 0 ? 1 : filter.Page;
            var pageSize = filter.PageSize <= 0 ? 20 : filter.PageSize;
            var skip = (page - 1) * pageSize;

            var items = await query
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync()
                .ConfigureAwait(false);

            return (items, totalCount);
        }
        public async Task<Dictionary<int, int>> GetStatusSummaryAsync(TicketFilterDto filter)
        {
            // Usamos AsNoTracking para mejorar el rendimiento en conteos
            var query = _context.Tickets.AsNoTracking().AsQueryable();

            // ✅ Filtros dinámicos (Asegúrate de NO filtrar por Estado aquí si quieres el resumen de todas las pestañas)
            if (!string.IsNullOrWhiteSpace(filter.TextoBusqueda))
                query = query.Where(t => t.Asunto.Contains(filter.TextoBusqueda) || t.Descripcion.Contains(filter.TextoBusqueda));

            if (filter.AreaId.HasValue)
                query = query.Where(t => t.AreaId == filter.AreaId.Value);

            if (filter.UsuarioId.HasValue)
                query = query.Where(t => t.SolicitanteId == filter.UsuarioId.Value);

            if (filter.Prioridad.HasValue)
                query = query.Where(t => t.Prioridad == filter.Prioridad.Value);

            if (filter.TieneGestor.HasValue)
                query = filter.TieneGestor.Value
                    ? query.Where(t => t.GestorAsignadoId != null)
                    : query.Where(t => t.GestorAsignadoId == null);

            // ✅ La Clave: Agrupar por el valor entero del Enum
            // Esto genera un SQL: GROUP BY [Estado] (donde Estado es int)
            var result = await query
         .GroupBy(t => t.Estado) // Agrupamos por el Enum directamente
         .Select(g => new { Estado = g.Key, Cantidad = g.Count() })
         .ToListAsync();

            // ✅ PASO 2: Convertimos a Diccionario en memoria de C# (donde no habrá error de SQL)
            return result.ToDictionary(x => (int)x.Estado, x => x.Cantidad);
        }
    }
}