namespace MiniTicker.Core.Application.Catalogs
{
   // AreaDto.cs
public class AreaDto
{
    public Guid Id { get; set; }
    public string Nombre { get; set; }
    public string Prefijo { get; set; }
    public bool Activo { get; set; }
    public Guid? ResponsableId { get; set; } // <--- Nuevo campo
    public AreaStatsDto Stats { get; set; }   // <--- Nuevo objeto
}

public class AreaStatsDto
{
    public int Total { get; set; }
    public int Activas { get; set; }
    public int Completadas { get; set; }
}
}