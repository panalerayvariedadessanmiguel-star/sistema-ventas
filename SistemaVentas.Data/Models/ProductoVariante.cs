namespace SistemaVentas.Data.Models
{
    public class ProductoVariante
    {
        public int Id { get; set; }
        public int ProductoId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string ColorHex { get; set; } = "#000000";
        public string Talla { get; set; } = string.Empty;
        public int? Stock { get; set; }
        public string ImagenUrl { get; set; } = string.Empty;
        public bool Activo { get; set; } = true;
        public int Orden { get; set; }
    }
}
