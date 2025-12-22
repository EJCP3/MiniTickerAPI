using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MiniTicker.Infrastructure.Persistence;
using MiniTicker.Core.Application.Interfaces.Repositories;
using MiniTicker.Core.Domain.Entities;

namespace MiniTicker.Infrastructure.Persistence.Repositories
{
    internal class TipoSolicitudRepository : ITipoSolicitudRepository
    {
        private readonly ApplicationDbContext _context;

        public TipoSolicitudRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task AddAsync(TipoSolicitud tipoSolicitud)
        {
            if (tipoSolicitud == null) throw new ArgumentNullException(nameof(tipoSolicitud));

            await _context.TiposSolicitud.AddAsync(tipoSolicitud).ConfigureAwait(false);
            await _context.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task UpdateAsync(TipoSolicitud tipoSolicitud)
        {
            if (tipoSolicitud == null) throw new ArgumentNullException(nameof(tipoSolicitud));

            _context.TiposSolicitud.Update(tipoSolicitud);
            await _context.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task DeleteAsync(TipoSolicitud tipoSolicitud)
        {
            if (tipoSolicitud == null) throw new ArgumentNullException(nameof(tipoSolicitud));

            _context.TiposSolicitud.Remove(tipoSolicitud);
            await _context.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task<TipoSolicitud?> GetByIdAsync(Guid id)
        {
            return await _context.TiposSolicitud
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == id)
                .ConfigureAwait(false);
        }

        public async Task<IReadOnlyList<TipoSolicitud>> GetByAreaIdAsync(Guid areaId)
        {
            var list = await _context.TiposSolicitud
                .AsNoTracking()
                .Where(t => t.AreaId == areaId)
                .ToListAsync()
                .ConfigureAwait(false);

            return list;
        }
    }
}
