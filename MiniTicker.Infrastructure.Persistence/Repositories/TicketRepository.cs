using System;
using System.Collections.Generic;
using System.Linq;
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
                .FirstOrDefaultAsync(t => t.Id == ticketId)
                .ConfigureAwait(false);
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

        public async Task<(IReadOnlyList<Ticket> Tickets, int TotalCount)> GetPagedAsync(TicketFilterDto filter)
        {
            if (filter == null) throw new ArgumentNullException(nameof(filter));

            var query = _context.Tickets.AsNoTracking().AsQueryable();

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
                query = query.Where(t => t.FechaCreacion <= filter.FechaHasta.Value);
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
    }
}
