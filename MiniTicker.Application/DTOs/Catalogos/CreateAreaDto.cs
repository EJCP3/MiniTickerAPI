namespace MiniTicker.Core.Application.Catalogs
{
    public class CreateAreaDto
    {
        public string Nombre { get; set; } = string.Empty;

        public Guid? ResponsableId { get; set; }
    }
}