namespace SistemaVentas.WebAPI.DTOs;

public class LoginRequest
{
    public string Documento { get; set; } = string.Empty;
    public string Contrasena { get; set; } = string.Empty;
}

public class ChangePasswordRequest
{
    public string NuevaContrasena { get; set; } = string.Empty;
}
