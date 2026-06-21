namespace SistemaVentas.Data.Models
{
    public class ProductoCodigoBarras
    {
        public int Id { get; set; }
        public int ProductoId { get; set; }
        public string CodigoBarras { get; set; } = string.Empty;
    }
}
