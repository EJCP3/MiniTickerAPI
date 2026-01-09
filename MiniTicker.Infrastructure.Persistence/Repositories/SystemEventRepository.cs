using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore; // 👈 NECESARIO PARA .Include() y .ToListAsync()
using MiniTicker.Core.Application.Interfaces.Repositories;
using MiniTicker.Core.Domain.Entities; // 👈 NECESARIO PARA SystemEvent
using MiniTicker.Infrastructure.Persistence;

public class SystemEventRepository : ISystemEventRepository
{
    private readonly ApplicationDbContext _context;
    public SystemEventRepository(ApplicationDbContext context) => _context = context;

    public async Task AddAsync(SystemEvent evt)
    {
        await _context.Set<SystemEvent>().AddAsync(evt);
        await _context.SaveChangesAsync();
    }

    public async Task<IReadOnlyList<SystemEvent>> GetRecentAsync(int count = 20)
    {
        return await _context.Set<SystemEvent>()
            .Include(e => e.Usuario)
            .AsNoTracking()
            .OrderByDescending(e => e.Fecha)
            .Take(count)
            .ToListAsync();
    }
}