using Microsoft.AspNetCore.Mvc;
using SistemaVentas.WebAPI.DTOs;
using SistemaVentas.WebAPI.Models;
using SistemaVentas.WebAPI.Repositories;
using SistemaVentas.WebAPI.Helpers;
using SistemaVentas.WebAPI.Services;

namespace SistemaVentas.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VentasController : ControllerBase
{
    private readonly VentaRepository _ventaRepo;
    private readonly ProductoRepository _productoRepo;
    private readonly FacturacionService _facturacionService;
    private readonly ImpresionService _impresionService;
    private readonly NotificacionService _notificacionService;
    private readonly ILogger<VentasController> _logger;

    public VentasController(
        VentaRepository ventaRepo,
        ProductoRepository productoRepo,
        FacturacionService facturacionService,
        ImpresionService impresionService,
        NotificacionService notificacionService,
        ILogger<VentasController> logger)
    {
        _ventaRepo = ventaRepo;
        _productoRepo = productoRepo;
        _facturacionService = facturacionService;
        _impresionService = impresionService;
        _notificacionService = notificacionService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var ventas = await _ventaRepo.GetAllAsync();
        return Ok(ventas);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var venta = await _ventaRepo.GetByIdAsync(id);
        if (venta == null) return NotFound();

        var detalles = await _ventaRepo.GetDetallesByVentaIdAsync(id);
        return Ok(new { venta, detalles });
    }

    [HttpPost]
    public async Task<IActionResult> Registrar([FromBody] RegistrarVentaDto dto)
    {
        if (dto.Detalles == null || dto.Detalles.Count == 0)
            return BadRequest("La venta debe tener al menos un producto");

        foreach (var det in dto.Detalles)
        {
            var producto = await _productoRepo.GetByIdAsync(det.ProductoId);
            if (producto == null)
                return BadRequest($"El producto ID {det.ProductoId} no existe");
            if (producto.Stock < det.Cantidad)
                return BadRequest($"Stock insuficiente para {producto.Nombre}. Disponible: {producto.Stock}");
        }

        var numeroVenta = await _ventaRepo.GenerarNumeroVentaAsync();

        var venta = new Venta
        {
            NumeroVenta = numeroVenta,
            CajaId = 1,
            ClienteId = dto.ClienteId,
            FechaVenta = DateTime.Now,
            SubTotal = dto.Detalles.Sum(d => d.PrecioUnitario * d.Cantidad),
            Impuesto = 0,
                Total = dto.Detalles.Sum(d => d.PrecioUnitario * d.Cantidad) + dto.Domicilio,
                MetodoPago = dto.MetodoPago,
                MontoPagado = dto.MontoPagado,
                Cambio = dto.Cambio,
                Usuario = dto.Usuario,
                Origen = string.IsNullOrEmpty(dto.Origen) ? "Web" : dto.Origen,
                Estado = dto.Origen == "Fisico" ? "Confirmada" : "Pendiente",
                Domicilio = dto.Domicilio
            };

        var detalles = dto.Detalles.Select(d => new DetalleVenta
        {
            ProductoId = d.ProductoId,
            ProductoVarianteId = d.ProductoVarianteId,
            Cantidad = d.Cantidad,
            PrecioUnitario = d.PrecioUnitario,
            CostoUnitario = d.CostoUnitario,
            SubTotal = d.PrecioUnitario * d.Cantidad,
            Impuesto = 0,
            Total = d.PrecioUnitario * d.Cantidad
        }).ToList();

        var ventaId = await _ventaRepo.InsertWithDetailsAsync(venta, detalles);
        venta.Id = ventaId;

        return CreatedAtAction(nameof(GetById), new { id = ventaId }, venta);
    }

    [HttpGet("sincronizar")]
    public async Task<IActionResult> GetVentasSincronizacion([FromQuery] DateTime ultimaSync)
    {
        var ventas = await _ventaRepo.GetVentasSincronizacionAsync(ultimaSync);
        return Ok(ventas);
    }

    [HttpGet("mis-pedidos")]
    public async Task<IActionResult> GetMisPedidos([FromHeader] string? xClienteToken)
    {
        var clienteId = AdminAuth.GetClienteId(xClienteToken);
        if (clienteId == null)
            return Unauthorized(new { mensaje = "Se requiere autenticacion de cliente" });

        var ventas = await _ventaRepo.GetByClienteIdAsync(clienteId.Value);
        return Ok(ventas);
    }

    [HttpPut("{id}/anular")]
    public async Task<IActionResult> Anular(int id, [FromBody] AnularVentaDto dto, [FromHeader] string? xAdminToken)
    {
        var userId = Helpers.AdminAuth.GetUserId(xAdminToken);
        if (userId == null) return Unauthorized(new { mensaje = "Se requiere autenticacion de administrador" });

        var venta = await _ventaRepo.GetByIdAsync(id);
        if (venta == null) return NotFound();
        if (venta.Anulada) return BadRequest(new { mensaje = "La venta ya esta anulada" });

        await _ventaRepo.AnularAsync(id, dto.Motivo);
        return Ok(new { mensaje = "Venta anulada exitosamente" });
    }

    [HttpGet("pendientes")]
    public async Task<IActionResult> GetPendientes([FromHeader] string? xAdminToken)
    {
        var userId = Helpers.AdminAuth.GetUserId(xAdminToken);
        if (userId == null) return Unauthorized(new { mensaje = "Se requiere autenticacion de administrador" });

        var ventas = await _ventaRepo.GetPendientesAsync();
        return Ok(ventas);
    }

    [HttpPatch("{id}/confirmar-pago")]
    public async Task<IActionResult> ConfirmarPago(int id, [FromHeader] string? xAdminToken)
    {
        var userId = Helpers.AdminAuth.GetUserId(xAdminToken);
        if (userId == null) return Unauthorized(new { mensaje = "Se requiere autenticacion de administrador" });

        var ok = await _ventaRepo.ConfirmarPagoAsync(id);
        if (!ok) return BadRequest(new { mensaje = "La venta no esta pendiente o no existe" });

        var venta = await _ventaRepo.GetByIdAsync(id);
        var detalles = await _ventaRepo.GetDetallesByVentaIdAsync(id);
        if (venta != null && detalles != null && detalles.Count > 0)
        {
            _logger.LogInformation("Generando factura para venta {Numero} ({Id})", venta.NumeroVenta, id);

            string rutaFactura;
            try
            {
                rutaFactura = await _facturacionService.GenerarFacturaAsync(venta, detalles.ToList());
                _logger.LogInformation("Factura generada en: {Ruta}", rutaFactura);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar factura para venta {Numero}", venta.NumeroVenta);
                return Ok(new { mensaje = "Pago confirmado, pero hubo un error al generar la factura", error = ex.Message });
            }

            _ = Task.Run(async () =>
            {
                try
                {
                    await _notificacionService.NotificarPedidoConfirmadoAsync(venta, detalles.ToList());
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al enviar notificacion para venta {Numero}", venta.NumeroVenta);
                }
            });

            _ = Task.Run(async () =>
            {
                try
                {
                    _logger.LogInformation("Iniciando impresion para {Numero}", venta.NumeroVenta);
                    var impreso = await _impresionService.ImprimirFacturaAsync(rutaFactura, venta, detalles.ToList());
                    _logger.LogInformation("Resultado impresion: {Resultado}", impreso ? "Exitoso" : "Fallido");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error en impresion para {Numero}", venta.NumeroVenta);
                }
            });

            return Ok(new
            {
                mensaje = "Pago confirmado exitosamente",
                factura = new
                {
                    ruta = rutaFactura,
                    nombreArchivo = Path.GetFileName(rutaFactura),
                    ventaId = venta.Id,
                    numeroVenta = venta.NumeroVenta,
                    total = venta.Total
                }
            });
        }

        _logger.LogWarning("Venta {Id}: no se encontraron detalles para facturar", id);
        return Ok(new { mensaje = "Pago confirmado exitosamente" });
    }

    [HttpGet("pendientes-pos")]
    public async Task<IActionResult> GetPendientesPOS([FromHeader] string? xPosToken)
    {
        if (!AdminAuth.ValidatePosToken(xPosToken))
            return Unauthorized(new { mensaje = "Token POS invalido" });
        var ventas = await _ventaRepo.GetPendientesPOSAsync();
        return Ok(ventas);
    }

    [HttpPost("marcar-sincronizado/{syncId}")]
    public async Task<IActionResult> MarcarPOSSync(int syncId, [FromHeader] string? xPosToken)
    {
        if (!AdminAuth.ValidatePosToken(xPosToken))
            return Unauthorized(new { mensaje = "Token POS invalido" });
        var ok = await _ventaRepo.MarcarPOSSyncAsync(syncId);
        if (!ok) return NotFound();
        return Ok(new { mensaje = "Sincronizado marcado exitosamente" });
    }

    [HttpPatch("{id}/origen")]
    public async Task<IActionResult> UpdateOrigen(int id, [FromBody] UpdateOrigenDto dto, [FromHeader] string? xAdminToken)
    {
        var userId = Helpers.AdminAuth.GetUserId(xAdminToken);
        if (userId == null) return Unauthorized(new { mensaje = "Se requiere autenticacion de administrador" });

        var venta = await _ventaRepo.GetByIdAsync(id);
        if (venta == null) return NotFound();

        await _ventaRepo.UpdateOrigenAsync(id, dto.Origen);
        return Ok(new { mensaje = "Origen actualizado exitosamente" });
    }

    [HttpPatch("{id}/numero-venta")]
    public async Task<IActionResult> UpdateNumeroVenta(int id, [FromBody] UpdateNumeroVentaDto dto, [FromHeader] string? xAdminToken)
    {
        var userId = Helpers.AdminAuth.GetUserId(xAdminToken);
        if (userId == null) return Unauthorized(new { mensaje = "Se requiere autenticacion de administrador" });

        var venta = await _ventaRepo.GetByIdAsync(id);
        if (venta == null) return NotFound();

        await _ventaRepo.UpdateNumeroVentaAsync(id, dto.NumeroVenta);
        return Ok(new { mensaje = "Numero de venta actualizado exitosamente" });
    }
}
