namespace MiniTicker.Core.Application.Catalogs
{
    public class TipoSolicitudDto
    {
        public Guid Id { get; set; }
        public string Nombre { get; set; } = null!;
        public Guid AreaId { get; set; }
        public bool Activo { get; set; }
    }
}