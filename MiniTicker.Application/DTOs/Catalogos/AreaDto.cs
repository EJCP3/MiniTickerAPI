namespace MiniTicker.Core.Application.Catalogs
{
    public class AreaDto
    {
        public Guid Id { get; set; }
        public string Nombre { get; set; } = null!;
        public bool Activo { get; set; }
    }
}