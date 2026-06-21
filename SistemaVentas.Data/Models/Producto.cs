using System;
using System.Collections.Generic;

namespace SistemaVentas.Data.Models
{
    public class Producto
    {
        public int Id { get; set; }
        public string CodigoBarras { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public int CategoriaId { get; set; }
        public decimal PrecioCompra { get; set; }
        public decimal PrecioVenta { get; set; }
        public int Stock { get; set; }
        public int StockMinimo { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime FechaModificacion { get; set; }
    public bool Activo { get; set; }
    public string NombreCategoria { get; set; } = string.Empty;
    public List<ProductoCodigoBarras> CodigosBarras { get; set; } = new List<ProductoCodigoBarras>();
    }
}
