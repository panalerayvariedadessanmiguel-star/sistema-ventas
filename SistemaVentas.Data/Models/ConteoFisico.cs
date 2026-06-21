using System;
using System.Collections.Generic;

namespace SistemaVentas.Data.Models
{
    public class ConteoFisico
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        public string Usuario { get; set; }
        public string Observaciones { get; set; }
        public string Estado { get; set; }
        public decimal ValorFaltante { get; set; }
        public decimal ValorSobrante { get; set; }
        public int TipoConteo { get; set; } = 1;
        public int? ConteoOriginalId { get; set; }
        public string ConteoOriginalDesc { get; set; }
        public List<DetalleConteoFisico> Detalles { get; set; } = new List<DetalleConteoFisico>();
    }

    public class DetalleConteoFisico
    {
        public int Id { get; set; }
        public int ConteoId { get; set; }
        public int ProductoId { get; set; }
        public int StockSistema { get; set; }
        public int StockFisico { get; set; }
        public int Diferencia { get; set; }
        public decimal ValorFaltante { get; set; }
        public decimal ValorSobrante { get; set; }
        public DateTime FechaRegistro { get; set; }

        public string CodigoBarras { get; set; }
        public string NombreProducto { get; set; }
        public decimal PrecioCompra { get; set; }

        public int? StockFisicoOriginal { get; set; }
        public int? DiferenciaOriginal { get; set; }
    }
}
