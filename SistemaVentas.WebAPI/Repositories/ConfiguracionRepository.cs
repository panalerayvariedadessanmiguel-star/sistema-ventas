using Dapper;
using SistemaVentas.WebAPI.Data;
using SistemaVentas.WebAPI.Models;

namespace SistemaVentas.WebAPI.Repositories;

public class ConfiguracionRepository
{
    private static readonly string[] DesignKeys = [
        "NOMBRE_EMPRESA", "COLOR_PRINCIPAL", "COLOR_SECUNDARIO", "COLOR_FONDO",
        "SLOGAN", "HERO_TITLE", "HERO_SUBTITLE",
        "SITE_TITLE", "SITE_SUBTITLE", "SITE_LOGO",
        "INFO_TELEFONO", "INFO_DIRECCION", "INFO_CIUDAD", "INFO_EMAIL", "INFO_SITIO_WEB", "INFO_HORARIO", "INFO_WHATSAPP", "INFO_NIT",
        "DOMICILIO_COSTO", "DOMICILIO_GRATIS_DESDE", "DOMICILIO_TIEMPO_ESTIMADO",
        "QR_BREB_IMG",
        "BREB_LLAVE",
        "BANCO_NOMBRE", "BANCO_TIPO_CUENTA", "BANCO_NUMERO_CUENTA", "BANCO_TITULAR",
        "TARJETA_INFO",
        "TAWKTO_PROPERTY_ID"
    ];

    private static readonly string[] DesignDefaults = [
        "Panalera y Variedades San Miguel", "#3B82F6", "#059669", "#F9FAFB",
        "Tu tienda de confianza", "Los mejores productos para ti", "Encuentra todo lo que necesitas",
        "Bienvenido a Nuestra Tienda", "Descubre nuestros productos exclusivos", "",
        "", "", "", "", "", "", "", "",
        "5000", "0", "2-5 dias habiles",
        "",
        "",
        "", "Ahorros", "", "",
        "Paga con tarjeta debito o credito en la entrega",
        ""
    ];

    private static readonly string[] DesignDescriptions = [
        "Nombre de la empresa", "Color principal de la tienda (hex)", "Color secundario (hex)", "Color de fondo de la pagina (hex)",
        "Eslogan debajo del nombre del negocio", "Hero - Titulo principal", "Hero - Subtitulo",
        "Titulo alternativo de la pagina de inicio", "Subtitulo de la pagina de inicio", "URL del logo (opcional)",
        "Telefono de contacto", "Direccion", "Ciudad", "Correo electronico", "Sitio web", "Horario de atencion", "WhatsApp", "NIT de la empresa",
        "Costo de domicilio (COP)", "Domicilio gratis desde (COP, 0 = nunca gratis)", "Tiempo estimado de entrega (ej: 2-5 dias habiles)",
        "URL de la imagen del codigo QR Bre-B (Davivienda)",
        "Llave Bre-B (numero de telefono, correo o documento)",
        "Nombre del banco", "Tipo de cuenta bancaria", "Numero de cuenta bancaria", "Titular de la cuenta",
        "Informacion para pago con tarjeta",
        "ID de propiedad de Tawk.to (dejar vacio si no se usa)"
    ];

    private readonly DbConnection _db;

    public ConfiguracionRepository(DbConnection db)
    {
        _db = db;
    }

    public async Task<List<Configuracion>> GetAllAsync()
    {
        using var conn = _db.GetConnection();
        return (await conn.QueryAsync<Configuracion>("SELECT * FROM Configuracion ORDER BY Id")).AsList();
    }

    public async Task<bool> UpdateAsync(string clave, string valor)
    {
        using var conn = _db.GetConnection();
        var rows = await conn.ExecuteAsync(
            "UPDATE Configuracion SET Valor = @Valor WHERE Clave = @Clave",
            new { Clave = clave, Valor = valor });
        return rows > 0;
    }

    public async Task UpdateBatchAsync(List<(string Clave, string Valor)> items)
    {
        using var conn = _db.GetConnection();
        foreach (var item in items)
        {
            await conn.ExecuteAsync(
                "UPDATE Configuracion SET Valor = @Valor WHERE Clave = @Clave",
                new { Clave = item.Clave, Valor = item.Valor });
        }
    }

    public async Task SeedDesignDefaultsAsync()
    {
        using var conn = _db.GetConnection();
        for (int i = 0; i < DesignKeys.Length; i++)
        {
            var exists = await conn.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM Configuracion WHERE Clave = @Clave", new { Clave = DesignKeys[i] });
            if (exists == 0)
            {
                await conn.ExecuteAsync(
                    "INSERT INTO Configuracion (Clave, Valor, Descripcion) VALUES (@Clave, @Valor, @Descripcion)",
                    new { Clave = DesignKeys[i], Valor = DesignDefaults[i], Descripcion = DesignDescriptions[i] });
            }
        }
    }

    public async Task<List<Configuracion>> GetDesignConfigAsync()
    {
        using var conn = _db.GetConnection();
        return (await conn.QueryAsync<Configuracion>(
            "SELECT * FROM Configuracion WHERE Clave LIKE 'COLOR_%' OR Clave LIKE 'SITE_%' OR Clave LIKE 'NOMBRE_%' OR Clave LIKE 'QR_%' OR Clave LIKE 'BREB_%' OR Clave LIKE 'BANCO_%' OR Clave LIKE 'TARJETA_%' OR Clave LIKE 'TAWKTO_%' OR Clave LIKE 'HERO_%' OR Clave LIKE 'INFO_%' OR Clave LIKE 'DOMICILIO_%' OR Clave = 'SLOGAN' ORDER BY Id")).AsList();
    }
}
