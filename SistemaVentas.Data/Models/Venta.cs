using System;

namespace SistemaVentas.Data.Models
{
    public class Venta
    {
        public int Id { get; set; }
        public string NumeroVenta { get; set; } = string.Empty;
        public int CajaId { get; set; }
        public int? ClienteId { get; set; }
        public DateTime FechaVenta { get; set; }
        public decimal SubTotal { get; set; }
        public decimal Impuesto { get; set; }
        public decimal Total { get; set; }
        public string MetodoPago { get; set; } = string.Empty;
        public decimal MontoPagado { get; set; }
        public decimal Cambio { get; set; }
        public string Usuario { get; set; } = string.Empty;
        public bool Anulada { get; set; }
        public string MotivoAnulacion { get; set; } = string.Empty;
        public string NombreCliente { get; set; } = string.Empty;
        public string TipoDocumentoCliente { get; set; } = string.Empty;
        public string DocumentoCliente { get; set; } = string.Empty;
        public string Estado { get; set; } = "Confirmada";
    }
}
