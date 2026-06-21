namespace SistemaVentas.Data.Models
{
    public class DetalleVenta
    {
        public int Id { get; set; }
        public int VentaId { get; set; }
        public int ProductoId { get; set; }
        public int? ProductoVarianteId { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal CostoUnitario { get; set; }
        public decimal SubTotal { get; set; }
        public decimal Impuesto { get; set; }
        public decimal Total { get; set; }
        public string NombreProducto { get; set; } = string.Empty;
        public string NombreVariante { get; set; } = string.Empty;
    }
}
