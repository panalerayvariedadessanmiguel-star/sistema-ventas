namespace SistemaVentas.WebAPI.Models;

public class Categoria
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; }
    public bool Activo { get; set; }
}
