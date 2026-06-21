namespace SistemaVentas.WebAPI.DTOs;

public class CreateProductoDto
{
    public string CodigoBarras { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public int CategoriaId { get; set; }
    public decimal PrecioCompra { get; set; }
    public decimal PrecioVenta { get; set; }
    public int Stock { get; set; }
    public int StockMinimo { get; set; }
    public string ImagenUrl { get; set; } = string.Empty;
    public int Orden { get; set; }
}

public class UpdateProductoDto
{
    public string CodigoBarras { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public int CategoriaId { get; set; }
    public decimal PrecioCompra { get; set; }
    public decimal PrecioVenta { get; set; }
    public int Stock { get; set; }
    public int StockMinimo { get; set; }
    public string ImagenUrl { get; set; } = string.Empty;
    public int Orden { get; set; }
    public bool Activo { get; set; }
}

public class CreateCategoriaDto
{
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
}

public class UpdateCategoriaDto
{
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public bool Activo { get; set; }
}
