namespace MiniTicker.Core.Application.DTOs.Dashboard
{
    public class DashboardStatsDto
    {
        public DashboardKpisDto Kpis { get; set; } = new();
        public List<TendenciaDto> Tendencia { get; set; } = new();
        public List<EstatusStatDto> Estatus { get; set; } = new();
        public List<PrioridadStatDto> Prioridades { get; set; } = new();
        public List<TopSolicitanteDto> TopSolicitantes { get; set; } = new();
        public List<DesempenoAreaDto> DesempenoAreas { get; set; } = new();
        public List<TiempoAreaDto> TiemposPorArea { get; set; } = new();
    }

    public class DashboardKpisDto
    {
        public int Total { get; set; }
        public int Completadas { get; set; }
        public int Pendientes { get; set; }
        public int EnProceso { get; set; }
        public double TasaResolucion { get; set; } // Porcentaje
        public double TiempoPromedio { get; set; } // Días
        public double Satisfaccion { get; set; } // 1-5
    }

    public class TendenciaDto
    {
        public string Mes { get; set; } = string.Empty;
        public int Total { get; set; }
        public int Completadas { get; set; }
    }

    public class EstatusStatDto
    {
        public string Estado { get; set; } = string.Empty;
        public int Cantidad { get; set; }
    }

    public class PrioridadStatDto
    {
        public string Prioridad { get; set; } = string.Empty;
        public int Cantidad { get; set; }
    }

    public class TopSolicitanteDto
    {
        public string Id { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public int Cantidad { get; set; }
    }

    public class DesempenoAreaDto
    {
        public string Area { get; set; } = string.Empty;
        public int Total { get; set; }
        public int Completadas { get; set; }
    }

    public class TiempoAreaDto
    {
        public string Area { get; set; } = string.Empty;
        public double Dias { get; set; }
    }
}