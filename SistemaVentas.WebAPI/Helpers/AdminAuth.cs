namespace SistemaVentas.WebAPI.Helpers;

public static class AdminAuth
{
    public const string PosSharedSecret = "S1st3m4V3nt4s-P05-2024";

    public static int? GetUserId(string? token)
    {
        if (string.IsNullOrEmpty(token)) return null;
        try
        {
            var bytes = Convert.FromBase64String(token);
            var parts = System.Text.Encoding.UTF8.GetString(bytes).Split(':');
            if (parts.Length == 2 && parts[1] == "Administrador" && int.TryParse(parts[0], out var id))
                return id;
        }
        catch { }
        return null;
    }

    public static int? GetClienteId(string? token)
    {
        if (string.IsNullOrEmpty(token)) return null;
        try
        {
            var bytes = Convert.FromBase64String(token);
            var parts = System.Text.Encoding.UTF8.GetString(bytes).Split(':');
            if (parts.Length == 2 && parts[1] == "Cliente" && int.TryParse(parts[0], out var id))
                return id;
        }
        catch { }
        return null;
    }

    public static bool ValidatePosToken(string? token)
    {
        return !string.IsNullOrEmpty(token) && token == PosSharedSecret;
    }

    public static string GenerateToken(int userId, string role)
    {
        var raw = $"{userId}:{role}";
        return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(raw));
    }
}
