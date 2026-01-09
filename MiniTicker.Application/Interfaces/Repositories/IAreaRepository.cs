using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MiniTicker.Core.Domain.Entities;

namespace MiniTicker.Core.Application.Interfaces.Repositories
{
    public interface IAreaRepository
    {
        Task AddAsync(Area area);
        Task UpdateAsync(Area area);
        Task DeleteAsync(Area area);
        Task<Area?> GetByIdAsync(Guid id);
        Task<IReadOnlyList<Area>> GetAllAsync(bool incluirInactivos = false, CancellationToken cancellationToken = default);
    }
}