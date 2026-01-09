using MiniTicker.Core.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MiniTicker.Core.Application.Interfaces.Repositories
{
    public interface ISystemEventRepository
    {
        Task AddAsync(SystemEvent evt);
        // Método para obtener los recientes para el dashboard
        Task<IReadOnlyList<SystemEvent>> GetRecentAsync(int count = 20);
    }
}