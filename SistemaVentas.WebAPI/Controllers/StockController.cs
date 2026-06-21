using Microsoft.AspNetCore.Mvc;
using SistemaVentas.WebAPI.Repositories;

namespace SistemaVentas.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StockController : ControllerBase
{
    private readonly StockRepository _stockRepo;

    public StockController(StockRepository stockRepo)
    {
        _stockRepo = stockRepo;
    }

    [HttpGet("mes/{año}/{mes}")]
    public async Task<IActionResult> GetStockMes(int año, int mes)
    {
        var stock = await _stockRepo.GetStockMesAsync(año, mes);
        return Ok(stock);
    }

    [HttpPost("traspaso")]
    public async Task<IActionResult> EjecutarTraspaso([FromBody] TraspasoRequest? request = null)
    {
        int añoDestino, mesDestino;

        if (request != null && request.Año.HasValue && request.Mes.HasValue)
        {
            añoDestino = request.Año.Value;
            mesDestino = request.Mes.Value;
        }
        else
        {
            var ahora = DateTime.Now;
            añoDestino = ahora.Year;
            mesDestino = ahora.Month;
        }

        int añoAnterior = mesDestino == 1 ? añoDestino - 1 : añoDestino;
        int mesAnterior = mesDestino == 1 ? 12 : mesDestino - 1;

        bool yaExiste = await _stockRepo.ExisteStockParaMesAsync(añoDestino, mesDestino);
        if (yaExiste)
        {
            return Ok(new { 
                mensaje = $"Ya existe stock para {GetNombreMes(mesDestino)} {añoDestino}. No se realizó traspaso.",
                productosTrasladados = 0,
                añoDestino,
                mesDestino
            });
        }

        int productosTrasladados = await _stockRepo.EjecutarTraspasoMasivoAsync(añoAnterior, mesAnterior, añoDestino, mesDestino);

        return Ok(new { 
            mensaje = $"Traspaso completado: {productosTrasladados} productos trasladados de {GetNombreMes(mesAnterior)} {añoAnterior} a {GetNombreMes(mesDestino)} {añoDestino}.",
            productosTrasladados,
            añoAnterior,
            mesAnterior,
            añoDestino,
            mesDestino
        });
    }

    [HttpPost("traspaso-automatico")]
    public async Task<IActionResult> TraspasoAutomatico()
    {
        return await EjecutarTraspaso(null);
    }

    private static string GetNombreMes(int mes)
    {
        string[] meses = { "", "Enero", "Febrero", "Marzo", "Abril", "Mayo", "Junio",
                         "Julio", "Agosto", "Septiembre", "Octubre", "Noviembre", "Diciembre" };
        return mes >= 1 && mes <= 12 ? meses[mes] : "";
    }
}

public class TraspasoRequest
{
    public int? Año { get; set; }
    public int? Mes { get; set; }
}
