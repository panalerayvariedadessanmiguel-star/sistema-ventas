using System;

namespace SistemaVentas.Data.Models
{
    public class InventarioMovimiento
    {
        public int Id { get; set; }
        public int ProductoId { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public int StockAnterior { get; set; }
        public int StockNuevo { get; set; }
        public string Motivo { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
        public string Usuario { get; set; } = string.Empty;
        public string NombreProducto { get; set; } = string.Empty;
    }
}
