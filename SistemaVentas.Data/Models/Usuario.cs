using System;

namespace SistemaVentas.Data.Models
{
    public class Usuario
    {
        public int Id { get; set; }
        public string Nombres { get; set; } = string.Empty;
        public string Apellidos { get; set; } = string.Empty;
        public string Documento { get; set; } = string.Empty;
        public string TipoDocumento { get; set; } = string.Empty;
        public string Contraseña { get; set; } = string.Empty;
        public string Rol { get; set; } = "Cajero";
        public decimal? Salario { get; set; }
        public bool Activo { get; set; } = true;
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
    }
}
