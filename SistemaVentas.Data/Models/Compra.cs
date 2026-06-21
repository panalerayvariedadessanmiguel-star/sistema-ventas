using System;

namespace SistemaVentas.Data.Models
{
    public class Compra
    {
        public int Id { get; set; }
        public string NumeroCompra { get; set; } = string.Empty;
        public DateTime FechaCompra { get; set; }
        public string Proveedor { get; set; } = string.Empty;
        public decimal SubTotal { get; set; }
        public decimal Impuesto { get; set; }
        public decimal Total { get; set; }
        public string Usuario { get; set; } = string.Empty;
    }
}
