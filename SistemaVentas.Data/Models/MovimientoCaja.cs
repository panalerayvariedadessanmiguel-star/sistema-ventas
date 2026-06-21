using System;

namespace SistemaVentas.Data.Models
{
    public class MovimientoCaja
    {
        public int Id { get; set; }
        public int CajaId { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public string Concepto { get; set; } = string.Empty;
        public decimal Monto { get; set; }
        public DateTime Fecha { get; set; }
        public string Usuario { get; set; } = string.Empty;
    }
}
