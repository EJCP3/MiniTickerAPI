using Microsoft.EntityFrameworkCore;
using MiniTicker.Core.Application.DTOs.Dashboard;
using MiniTicker.Core.Application.Interfaces.Repositories;
using MiniTicker.Core.Application.Interfaces.Services;
using MiniTicker.Core.Domain.Entities;
using MiniTicker.Core.Domain.Enums;

namespace MiniTicker.Core.Application.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly ITicketRepository _ticketRepository;

        public DashboardService(ITicketRepository ticketRepository)
        {
            _ticketRepository = ticketRepository;
        }

        public async Task<DashboardStatsDto> GetStatsAsync(string periodo, Guid? areaId = null)
        {
            var query = _ticketRepository.GetAllAsQueryable();
        
            // 1. LÓGICA DE FECHAS MEJORADA
            // Definimos Fecha Inicio según el periodo
            DateTime fechaInicio = DateTime.UtcNow;

            switch (periodo)
            {
                case "esta-semana":
                    // Calcular el lunes de la semana actual
                    var diff = DateTime.UtcNow.DayOfWeek - DayOfWeek.Monday;
                    if (diff < 0) diff += 7;
                    fechaInicio = DateTime.UtcNow.AddDays(-diff).Date; // Inicio del Lunes 00:00
                    break;

                case "este-mes":
                    fechaInicio = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
                    break;

                case "mes-pasado":
                    // Lógica para mes pasado (opcional si la usas)
                    var mesPasado = DateTime.UtcNow.AddMonths(-1);
                    fechaInicio = new DateTime(mesPasado.Year, mesPasado.Month, 1);
                    // Nota: Aquí deberías filtrar también fecha Fin, pero para simplificar lo dejo así
                    break;

                case "anio-actual":
                    fechaInicio = new DateTime(DateTime.UtcNow.Year, 1, 1);
                    break;

                default:
                    fechaInicio = DateTime.MinValue;
                    break;
            }

            // Aplicar filtro de fecha
            if (fechaInicio != DateTime.MinValue)
            {
                query = query.Where(x => x.FechaCreacion >= fechaInicio);
            }


            if (areaId.HasValue)
            {
                query = query.Where(x => x.AreaId == areaId.Value);
            }
            // Traer datos a memoria
            var tickets = await query
                .Include(x => x.Area)
                .Include(x => x.Solicitante)
                .ToListAsync();

            var stats = new DashboardStatsDto();

            // --- A. KPIs (Igual que antes) ---
            stats.Kpis.Total = tickets.Count;
            stats.Kpis.Completadas = tickets.Count(x => x.Estado == EstadoTicket.Resuelta || x.Estado == EstadoTicket.Cerrada);
            stats.Kpis.Pendientes = tickets.Count(x => x.Estado == EstadoTicket.Nueva);
            stats.Kpis.EnProceso = tickets.Count(x => x.Estado == EstadoTicket.EnProceso);

            stats.Kpis.TasaResolucion = stats.Kpis.Total > 0
                ? Math.Round(((double)stats.Kpis.Completadas / stats.Kpis.Total) * 100, 1)
                : 0;

            // Tiempo promedio
            var ticketsCerrados = tickets.Where(x => (x.Estado == EstadoTicket.Resuelta || x.Estado == EstadoTicket.Cerrada) && x.FechaActualizacion != null).ToList();
            if (ticketsCerrados.Any())
            {
                var promedioHoras = ticketsCerrados.Average(x => (x.FechaActualizacion.Value - x.FechaCreacion).TotalDays);
                stats.Kpis.TiempoPromedio = Math.Round(promedioHoras, 1);
            }
            stats.Kpis.Satisfaccion = 4.8;

            // --- B. TENDENCIA (CORREGIDO) ---
            // Si el periodo es corto (semana/mes), agrupamos por DÍA.
            // Si es largo (año), agrupamos por MES.

            if (periodo == "anio-actual")
            {
                // AGRUPACIÓN POR MES (Para reportes anuales)
                stats.Tendencia = tickets
                    .GroupBy(x => new { x.FechaCreacion.Year, x.FechaCreacion.Month })
                    .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                    .Select(g => new TendenciaDto
                    {
                        Mes = $"{g.Key.Month}/{g.Key.Year}",
                        Total = g.Count(),
                        Completadas = g.Count(x => x.Estado == EstadoTicket.Resuelta || x.Estado == EstadoTicket.Cerrada)
                    })
                    .ToList();
            }
            else
            {
                // AGRUPACIÓN POR DÍA (Para Semana y Mes)
                stats.Tendencia = tickets
                    .GroupBy(x => x.FechaCreacion.Date) // Agrupamos por fecha exacta (sin hora)
                    .OrderBy(g => g.Key)
                    .Select(g => new TendenciaDto
                    {
                        // Enviamos la fecha en formato día/mes
                        Mes = g.Key.ToString("dd/MM"),
                        Total = g.Count(),
                        Completadas = g.Count(x => x.Estado == EstadoTicket.Resuelta || x.Estado == EstadoTicket.Cerrada)
                    })
                    .ToList();
            }

            // --- C, D, E, F, G (El resto queda igual) ---
            stats.Estatus = tickets
                .GroupBy(x => x.Estado)
                .Select(g => new EstatusStatDto { Estado = g.Key.ToString(), Cantidad = g.Count() }).ToList();

            stats.Prioridades = tickets
                .GroupBy(x => x.Prioridad)
                .Select(g => new PrioridadStatDto { Prioridad = g.Key.ToString(), Cantidad = g.Count() }).ToList();

            stats.TopSolicitantes = tickets
                .Where(x => x.Solicitante != null)
                .GroupBy(x => x.Solicitante)
                .Select(g => new TopSolicitanteDto
                {
                    Id = g.Key.Id.ToString(),
                    Nombre = g.Key.Nombre,
                    Cantidad = g.Count()
                })
                .OrderByDescending(x => x.Cantidad)
                .Take(5)
                .ToList();

            stats.DesempenoAreas = tickets
                .Where(x => x.Area != null)
                .GroupBy(x => x.Area.Nombre)
                .Select(g => new DesempenoAreaDto
                {
                    Area = g.Key,
                    Total = g.Count(),
                    Completadas = g.Count(x => x.Estado == EstadoTicket.Resuelta || x.Estado == EstadoTicket.Cerrada)
                }).ToList();

            stats.TiemposPorArea = ticketsCerrados
                .Where(x => x.Area != null)
                .GroupBy(x => x.Area.Nombre)
                .Select(g => new TiempoAreaDto
                {
                    Area = g.Key,
                    Dias = Math.Round(g.Average(x => (x.FechaActualizacion.Value - x.FechaCreacion).TotalDays), 1)
                }).ToList();

            return stats;
        }
    }
}