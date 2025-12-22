using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MiniTicker.Infrastructure.Persistence;
using MiniTicker.Core.Application.Interfaces.Repositories;
using MiniTicker.Core.Domain.Entities;

namespace MiniTicker.Infrastructure.Persistence.Repositories
{
    internal class ComentarioRepository : IComentarioRepository
    {
        private readonly ApplicationDbContext _context;

        public ComentarioRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<Comentario> AddAsync(Comentario comentario, CancellationToken cancellationToken = default)
        {
            if (comentario == null) throw new ArgumentNullException(nameof(comentario));

            await _context.Comentarios.AddAsync(comentario, cancellationToken).ConfigureAwait(false);
            await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return comentario;
        }

        public async Task<IReadOnlyList<Comentario>> GetByTicketIdAsync(Guid ticketId, CancellationToken cancellationToken = default)
        {
            // Devuelve comentarios del ticket ordenados por Fecha ascendente (como solicitado)
            return await _context.Comentarios
                .AsNoTracking()
                .Where(c => c.TicketId == ticketId)
                .OrderBy(c => c.Fecha)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<IReadOnlyList<Comentario>> GetByTicketIdOrderedByFechaAscAsync(Guid ticketId, CancellationToken cancellationToken = default)
        {
            // Implementación explícita del método ordenado por Fecha ascendente
            return await _context.Comentarios
                .AsNoTracking()
                .Where(c => c.TicketId == ticketId)
                .OrderBy(c => c.Fecha)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }
    }
}