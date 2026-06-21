using System;

namespace SistemaVentas.Data.Models
{
    public class Caja
    {
        public int Id { get; set; }
        public int NumeroCaja { get; set; }
        public string Usuario { get; set; } = string.Empty;
        public decimal MontoInicial { get; set; }
        public DateTime FechaApertura { get; set; }
        public DateTime? FechaCierre { get; set; }
        public decimal? MontoCierreEsperado { get; set; }
        public decimal? MontoCierreReal { get; set; }
        public decimal? Diferencia { get; set; }
        public string ObservacionesApertura { get; set; } = string.Empty;
        public string ObservacionesCierre { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
    }
}
