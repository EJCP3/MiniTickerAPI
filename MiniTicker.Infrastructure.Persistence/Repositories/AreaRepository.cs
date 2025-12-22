using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MiniTicker.Infrastructure.Persistence;
using MiniTicker.Core.Application.Interfaces.Repositories;
using MiniTicker.Core.Domain.Entities;

namespace MiniTicker.Infrastructure.Persistence.Repositories
{
    internal class AreaRepository : IAreaRepository
    {
        private readonly ApplicationDbContext _context;

        public AreaRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task AddAsync(Area area)
        {
            if (area == null) throw new ArgumentNullException(nameof(area));

            await _context.Areas.AddAsync(area).ConfigureAwait(false);
            await _context.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task UpdateAsync(Area area)
        {
            if (area == null) throw new ArgumentNullException(nameof(area));

            _context.Areas.Update(area);
            await _context.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task DeleteAsync(Area area)
        {
            if (area == null) throw new ArgumentNullException(nameof(area));

            _context.Areas.Remove(area);
            await _context.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task<Area?> GetByIdAsync(Guid id)
        {
            return await _context.Areas
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == id)
                .ConfigureAwait(false);
        }

        public async Task<IReadOnlyList<Area>> GetAllAsync()
        {
            var list = await _context.Areas
                .AsNoTracking()
                .ToListAsync()
                .ConfigureAwait(false);

            return list;
        }
    }
}
