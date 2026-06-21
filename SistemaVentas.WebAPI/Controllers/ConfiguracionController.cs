using Microsoft.AspNetCore.Mvc;
using SistemaVentas.WebAPI.Helpers;
using SistemaVentas.WebAPI.Repositories;

namespace SistemaVentas.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ConfiguracionController : ControllerBase
{
    private readonly ConfiguracionRepository _repo;

    public ConfiguracionController(ConfiguracionRepository repo)
    {
        _repo = repo;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var config = await _repo.GetAllAsync();
        return Ok(config);
    }

    [HttpGet("public")]
    public async Task<IActionResult> GetPublic()
    {
        var config = await _repo.GetDesignConfigAsync();
        var dict = config.ToDictionary(c => c.Clave, c => c.Valor);
        return Ok(new
        {
            nombreTienda = dict.GetValueOrDefault("NOMBRE_EMPRESA", "Mi Tienda"),
            slogan = dict.GetValueOrDefault("SLOGAN", "Tu tienda de confianza"),
            heroTitle = dict.GetValueOrDefault("HERO_TITLE", "Los mejores productos para ti"),
            heroSubtitle = dict.GetValueOrDefault("HERO_SUBTITLE", "Encuentra todo lo que necesitas"),
            telefono = dict.GetValueOrDefault("INFO_TELEFONO", ""),
            direccion = dict.GetValueOrDefault("INFO_DIRECCION", ""),
            ciudad = dict.GetValueOrDefault("INFO_CIUDAD", ""),
            email = dict.GetValueOrDefault("INFO_EMAIL", ""),
            sitioWeb = dict.GetValueOrDefault("INFO_SITIO_WEB", ""),
            horario = dict.GetValueOrDefault("INFO_HORARIO", ""),
            whatsapp = dict.GetValueOrDefault("INFO_WHATSAPP", ""),
            nit = dict.GetValueOrDefault("INFO_NIT", ""),
            domicilioCosto = dict.GetValueOrDefault("DOMICILIO_COSTO", "5000"),
            domicilioGratisDesde = dict.GetValueOrDefault("DOMICILIO_GRATIS_DESDE", "0"),
            domicilioTiempoEstimado = dict.GetValueOrDefault("DOMICILIO_TIEMPO_ESTIMADO", "2-5 dias habiles"),
            colorPrincipal = dict.GetValueOrDefault("COLOR_PRINCIPAL", "#3B82F6"),
            colorSecundario = dict.GetValueOrDefault("COLOR_SECUNDARIO", "#059669"),
            colorFondo = dict.GetValueOrDefault("COLOR_FONDO", "#F9FAFB"),
            siteTitle = dict.GetValueOrDefault("SITE_TITLE", "Bienvenido"),
            siteSubtitle = dict.GetValueOrDefault("SITE_SUBTITLE", ""),
            logo = dict.GetValueOrDefault("SITE_LOGO", ""),
            qrBrebImg = dict.GetValueOrDefault("QR_BREB_IMG", ""),
            brebLlave = dict.GetValueOrDefault("BREB_LLAVE", ""),
            bancoNombre = dict.GetValueOrDefault("BANCO_NOMBRE", ""),
            bancoTipoCuenta = dict.GetValueOrDefault("BANCO_TIPO_CUENTA", "Ahorros"),
            bancoNumeroCuenta = dict.GetValueOrDefault("BANCO_NUMERO_CUENTA", ""),
            bancoTitular = dict.GetValueOrDefault("BANCO_TITULAR", ""),
            tarjetaInfo = dict.GetValueOrDefault("TARJETA_INFO", "Paga con tarjeta debito o credito en la entrega"),
            tawktoPropertyId = dict.GetValueOrDefault("TAWKTO_PROPERTY_ID", "")
        });
    }

    [HttpGet("diseno")]
    public async Task<IActionResult> GetDiseno([FromHeader] string? xAdminToken)
    {
        var userId = AdminAuth.GetUserId(xAdminToken);
        if (userId == null) return Unauthorized(new { mensaje = "Se requiere autenticacion de administrador" });
        var config = await _repo.GetDesignConfigAsync();
        return Ok(config);
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] List<UpdateConfigDto> items, [FromHeader] string? xAdminToken)
    {
        var userId = AdminAuth.GetUserId(xAdminToken);
        if (userId == null) return Unauthorized(new { mensaje = "Se requiere autenticacion de administrador" });

        await _repo.UpdateBatchAsync(items.Select(i => (i.Clave, i.Valor)).ToList());
        return Ok(new { mensaje = "Configuracion actualizada" });
    }
}

public class UpdateConfigDto
{
    public string Clave { get; set; } = string.Empty;
    public string Valor { get; set; } = string.Empty;
}
