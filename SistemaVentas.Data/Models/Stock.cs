using System;

namespace SistemaVentas.Data.Models
{
    public class Stock
    {
        public int Id { get; set; }
        public int ProductoId { get; set; }
        public int Año { get; set; }
        public int Mes { get; set; }
        public int CantidadInicial { get; set; }
        public int CantidadEntrante { get; set; }
        public int CantidadSaliente { get; set; }
        public int CantidadFinal { get; set; }
        public DateTime FechaRegistro { get; set; }

        // Propiedades de navegación
        public string NombreProducto { get; set; }
        public string CodigoBarras { get; set; }
    }
}
