using System;

namespace SistemaVentas.Data.Models
{
    public class Transaccion
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        public string Tipo { get; set; }
        public string Categoria { get; set; }
        public string Concepto { get; set; }
        public decimal Monto { get; set; }
        public string Usuario { get; set; }
        public DateTime FechaRegistro { get; set; }
    }
}
