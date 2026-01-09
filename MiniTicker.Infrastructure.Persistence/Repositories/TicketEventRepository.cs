using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MiniTicker.Core.Application.Interfaces.Repositories;
using MiniTicker.Core.Domain.Entities;
using MiniTicker.Infrastructure.Persistence;

namespace MiniTicker.Infrastructure.Persistence.Repositories
{
    public class TicketEventRepository : ITicketEventRepository
    {
        private readonly ApplicationDbContext _context;

        public TicketEventRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task AddAsync(TicketEvent evt, CancellationToken cancellationToken = default)
        {
            await _context.TicketEvents.AddAsync(evt, cancellationToken).ConfigureAwait(false);
            await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task<IReadOnlyList<TicketEvent>> GetByTicketIdOrderedAscAsync(Guid ticketId, CancellationToken cancellationToken = default)
        {
            return await _context.TicketEvents
                .Include(e => e.Usuario) 
                .AsNoTracking()
                .Where(e => e.TicketId == ticketId)
                .OrderBy(e => e.Fecha)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<IReadOnlyList<TicketEvent>> GetRecentByUserIdAsync(Guid userId, int count = 10)
        {
            return await _context.TicketEvents
                .Include(e => e.Ticket)  
                .Include(e => e.Usuario) 
                .AsNoTracking()
                .Where(e =>
                    e.UsuarioId == userId ||               
                    e.Ticket.SolicitanteId == userId       
                )
                .OrderByDescending(e => e.Fecha) 
                .Take(count)                     
                .ToListAsync();
        }

        public async Task<IReadOnlyList<TicketEvent>> GetGlobalRecentAsync(Guid? areaId = null, Guid? targetUserId = null, int count = 20)
        {
            var query = _context.TicketEvents
                .Include(e => e.Ticket)
                .Include(e => e.Usuario)
                .AsNoTracking();

            if (areaId.HasValue && areaId != Guid.Empty)
            {
                query = query.Where(e => e.Ticket.AreaId == areaId.Value);
            }

            if (targetUserId.HasValue && targetUserId != Guid.Empty)
            {
             
                query = query.Where(e => e.UsuarioId == targetUserId.Value);
            }

            return await query
                .OrderByDescending(e => e.Fecha)
                .Take(count)
                .ToListAsync();
        }
    }
}