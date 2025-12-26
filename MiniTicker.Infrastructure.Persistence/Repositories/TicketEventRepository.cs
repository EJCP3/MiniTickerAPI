using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MiniTicker.Core.Application.Interfaces.Repositories;
using MiniTicker.Core.Domain.Entities;

namespace MiniTicker.Infrastructure.Persistence.Repositories
{   
    public class TicketEventRepository : ITicketEventRepository
    {
        private readonly ApplicationDbContext _context;
        public TicketEventRepository(ApplicationDbContext context) => _context = context;

        public async Task AddAsync(TicketEvent evt, CancellationToken cancellationToken = default)
        {
            await _context.TicketEvents.AddAsync(evt, cancellationToken).ConfigureAwait(false);
            await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task<IReadOnlyList<TicketEvent>> GetByTicketIdOrderedAscAsync(Guid ticketId, CancellationToken cancellationToken = default)
        {
            return await _context.TicketEvents
                .AsNoTracking()
                .Where(e => e.TicketId == ticketId)
                .OrderBy(e => e.Fecha)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }
    }
}   