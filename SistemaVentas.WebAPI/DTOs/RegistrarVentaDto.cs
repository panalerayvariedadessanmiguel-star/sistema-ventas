namespace SistemaVentas.WebAPI.DTOs;

public class RegistrarVentaDto
{
    public int? ClienteId { get; set; }
    public string MetodoPago { get; set; } = "Efectivo";
    public decimal MontoPagado { get; set; }
    public decimal Cambio { get; set; }
    public string Usuario { get; set; } = "Web";
    public string Origen { get; set; } = "Web";
    public decimal Domicilio { get; set; }
    public List<DetalleVentaDto> Detalles { get; set; } = new();
}

public class DetalleVentaDto
{
    public int ProductoId { get; set; }
    public int? ProductoVarianteId { get; set; }
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal CostoUnitario { get; set; }
}

public class LoginDto
{
    public string Documento { get; set; } = string.Empty;
    public string Contrasena { get; set; } = string.Empty;
}

public class ChangePasswordDto
{
    public string NuevaContrasena { get; set; } = string.Empty;
}

public class AnularVentaDto
{
    public string Motivo { get; set; } = string.Empty;
}

public class UpdateOrigenDto
{
    public string Origen { get; set; } = string.Empty;
}

public class UpdateNumeroVentaDto
{
    public string NumeroVenta { get; set; } = string.Empty;
}
