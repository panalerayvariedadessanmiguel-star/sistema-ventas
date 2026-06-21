namespace SistemaVentas.Data.Models
{
    public class DetalleCompra
    {
        public int Id { get; set; }
        public int CompraId { get; set; }
        public int ProductoId { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal SubTotal { get; set; }
        public string NombreProducto { get; set; } = string.Empty;
    }
}
