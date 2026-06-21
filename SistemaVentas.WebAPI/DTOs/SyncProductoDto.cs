namespace SistemaVentas.WebAPI.DTOs;

public class SyncProductoVariantDto
{
    public int Id { get; set; }
    public int ProductoId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string ColorHex { get; set; } = "#000000";
    public string? Talla { get; set; }
    public int? Stock { get; set; }
    public string? ImagenUrl { get; set; }
    public bool Activo { get; set; } = true;
    public int Orden { get; set; }
}

public class SyncProductoDto
{
    public int LocalId { get; set; }
    public string CodigoBarras { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public int CategoriaId { get; set; }
    public string NombreCategoria { get; set; } = string.Empty;
    public decimal PrecioCompra { get; set; }
    public decimal PrecioVenta { get; set; }
    public int Stock { get; set; }
    public int StockMinimo { get; set; }
    public string ImagenUrl { get; set; } = string.Empty;
    public int Orden { get; set; }
    public bool Activo { get; set; }
    public DateTime? FechaModificacion { get; set; }
    public List<SyncProductoVariantDto>? Variantes { get; set; }
}

public class SyncResultDto
{
    public int LocalId { get; set; }
    public int RemoteId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public bool Creado { get; set; }
}
