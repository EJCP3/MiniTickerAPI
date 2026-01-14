using MiniTicker.Core.Application.DTOs.Dashboard;

namespace MiniTicker.Core.Application.Interfaces.Services
{
    public interface IDashboardService
    {
        // El parámetro 'periodo' puede servir para filtrar (mes, año, etc.)
        Task<DashboardStatsDto> GetStatsAsync(string periodo, Guid? areaId = null);
    }
}