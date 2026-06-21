using Microsoft.AspNetCore.Mvc;
using SistemaVentas.WebAPI.DTOs;
using SistemaVentas.WebAPI.Helpers;
using SistemaVentas.WebAPI.Models;
using SistemaVentas.WebAPI.Repositories;

namespace SistemaVentas.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductosController : ControllerBase
{
    private readonly ProductoRepository _productoRepo;
    private readonly CategoriaRepository _categoriaRepo;
    private readonly ProductoVarianteRepository _varianteRepo;
    private readonly ILogger<ProductosController> _logger;

    public ProductosController(ProductoRepository productoRepo, CategoriaRepository categoriaRepo, ProductoVarianteRepository varianteRepo, ILogger<ProductosController> logger)
    {
        _productoRepo = productoRepo;
        _categoriaRepo = categoriaRepo;
        _varianteRepo = varianteRepo;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var productos = await _productoRepo.GetAllAsync();
        return Ok(productos);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var producto = await _productoRepo.GetByIdAsync(id);
        if (producto == null) return NotFound();
        return Ok(producto);
    }

    [HttpGet("{id}/detalle")]
    public async Task<IActionResult> GetDetalle(int id)
    {
        var producto = await _productoRepo.GetByIdAsync(id);
        if (producto == null) return NotFound();
        var variantes = await _varianteRepo.GetByProductoAsync(id);

        var relacionados = await _productoRepo.GetByCategoriaAsync(producto.CategoriaId);
        relacionados = relacionados.Where(r => r.Id != id).Take(4).ToList();

        return Ok(new { producto, variantes, relacionados });
    }

    [HttpGet("categoria/{categoriaId}")]
    public async Task<IActionResult> GetByCategoria(int categoriaId)
    {
        var productos = await _productoRepo.GetByCategoriaAsync(categoriaId);
        return Ok(productos);
    }

    [HttpGet("buscar")]
    public async Task<IActionResult> Search([FromQuery] string q)
    {
        if (string.IsNullOrWhiteSpace(q)) return Ok(new List<Producto>());
        var productos = await _productoRepo.SearchAsync(q);
        return Ok(productos);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductoDto dto, [FromHeader] string? xAdminToken)
    {
        var userId = AdminAuth.GetUserId(xAdminToken);
        if (userId == null) return Unauthorized(new { mensaje = "Se requiere autenticacion de administrador" });

        var categoria = await _categoriaRepo.GetAllAsync();
        if (!categoria.Any(c => c.Id == dto.CategoriaId))
            return BadRequest("La categoria no existe");

        var producto = new Producto
        {
            CodigoBarras = dto.CodigoBarras,
            Nombre = dto.Nombre,
            Descripcion = dto.Descripcion,
            CategoriaId = dto.CategoriaId,
            PrecioCompra = dto.PrecioCompra,
            PrecioVenta = dto.PrecioVenta,
            Stock = dto.Stock,
            StockMinimo = dto.StockMinimo,
            ImagenUrl = dto.ImagenUrl,
            Orden = dto.Orden
        };

        var id = await _productoRepo.CreateAsync(producto);
        producto.Id = id;
        producto.FechaCreacion = DateTime.Now;
        producto.Activo = true;
        return CreatedAtAction(nameof(GetById), new { id }, producto);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateProductoDto dto, [FromHeader] string? xAdminToken)
    {
        var userId = AdminAuth.GetUserId(xAdminToken);
        if (userId == null) return Unauthorized(new { mensaje = "Se requiere autenticacion de administrador" });

        var existente = await _productoRepo.GetByIdAsync(id);
        if (existente == null) return NotFound();

        var producto = new Producto
        {
            Id = id,
            CodigoBarras = dto.CodigoBarras,
            Nombre = dto.Nombre,
            Descripcion = dto.Descripcion,
            CategoriaId = dto.CategoriaId,
            PrecioCompra = dto.PrecioCompra,
            PrecioVenta = dto.PrecioVenta,
            Stock = dto.Stock,
            StockMinimo = dto.StockMinimo,
            ImagenUrl = dto.ImagenUrl,
            Orden = dto.Orden,
            Activo = dto.Activo
        };

        await _productoRepo.UpdateAsync(producto);
        producto.NombreCategoria = existente.NombreCategoria;
        producto.FechaCreacion = existente.FechaCreacion;
        return Ok(producto);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id, [FromHeader] string? xAdminToken)
    {
        var userId = AdminAuth.GetUserId(xAdminToken);
        if (userId == null) return Unauthorized(new { mensaje = "Se requiere autenticacion de administrador" });

        var existente = await _productoRepo.GetByIdAsync(id);
        if (existente == null) return NotFound();

        await _productoRepo.DeleteAsync(id);
        return NoContent();
    }

    [HttpGet("variantes")]
    public async Task<IActionResult> GetAllVariantes()
    {
        var variantes = await _varianteRepo.GetAllAsync();
        return Ok(variantes);
    }

    [HttpGet("{id}/variantes")]
    public async Task<IActionResult> GetVariantes(int id)
    {
        var variantes = await _varianteRepo.GetByProductoAsync(id);
        return Ok(variantes);
    }

    [HttpPost("{id}/variantes")]
    public async Task<IActionResult> CreateVariante(int id, [FromBody] CreateVarianteDto dto, [FromHeader] string? xAdminToken)
    {
        var userId = AdminAuth.GetUserId(xAdminToken);
        if (userId == null) return Unauthorized(new { mensaje = "Se requiere autenticacion de administrador" });

        var existe = await _productoRepo.GetByIdAsync(id);
        if (existe == null) return NotFound(new { mensaje = "Producto no encontrado" });

        var variante = new ProductoVariante
        {
            ProductoId = id,
            Nombre = dto.Nombre,
            ColorHex = dto.ColorHex,
            Talla = dto.Talla,
            Stock = dto.Stock,
            ImagenUrl = dto.ImagenUrl,
            Orden = dto.Orden
        };

        var varianteId = await _varianteRepo.CreateAsync(variante);
        variante.Id = varianteId;
        return CreatedAtAction(nameof(GetVariantes), new { id }, variante);
    }

    [HttpPut("variantes/{varianteId}")]
    public async Task<IActionResult> UpdateVariante(int varianteId, [FromBody] UpdateVarianteDto dto, [FromHeader] string? xAdminToken)
    {
        var userId = AdminAuth.GetUserId(xAdminToken);
        if (userId == null) return Unauthorized(new { mensaje = "Se requiere autenticacion de administrador" });

        var variante = await _varianteRepo.GetByIdAsync(varianteId);
        if (variante == null) return NotFound(new { mensaje = "Variante no encontrada" });

        variante.Nombre = dto.Nombre;
        variante.ColorHex = dto.ColorHex;
        variante.Talla = dto.Talla;
        variante.Stock = dto.Stock;
        variante.ImagenUrl = dto.ImagenUrl;
        variante.Activo = dto.Activo;
        variante.Orden = dto.Orden;

        await _varianteRepo.UpdateAsync(variante);
        return Ok(variante);
    }

    [HttpDelete("variantes/{varianteId}")]
    public async Task<IActionResult> DeleteVariante(int varianteId, [FromHeader] string? xAdminToken)
    {
        var userId = AdminAuth.GetUserId(xAdminToken);
        if (userId == null) return Unauthorized(new { mensaje = "Se requiere autenticacion de administrador" });

        await _varianteRepo.DeleteAsync(varianteId);
        return NoContent();
    }

    [HttpPost("sync-pos")]
    public async Task<IActionResult> SyncFromPOS([FromBody] List<SyncProductoDto> productos)
    {
        var resultados = new List<SyncResultDto>();
        var categorias = await _categoriaRepo.GetAllAsync(includeInactive: true);

        foreach (var dto in productos)
        {
            try
            {
                var existente = string.IsNullOrEmpty(dto.CodigoBarras)
                    ? await _productoRepo.GetByNombreAsync(dto.Nombre)
                    : await _productoRepo.GetByCodigoBarrasAsync(dto.CodigoBarras);

                if (existente != null)
                {
                    existente.Nombre = dto.Nombre;
                    existente.Descripcion = dto.Descripcion;
                    existente.PrecioCompra = dto.PrecioCompra;
                    existente.PrecioVenta = dto.PrecioVenta;
                    existente.Stock = dto.Stock;
                    existente.StockMinimo = dto.StockMinimo;
                    if (!string.IsNullOrEmpty(dto.ImagenUrl)) existente.ImagenUrl = dto.ImagenUrl;
                    if (dto.Orden != 0) existente.Orden = dto.Orden;
                    existente.Activo = dto.Activo;
                    existente.CategoriaId = await ResolverCategoriaAsync(dto, categorias);

                    await _productoRepo.UpdateAsync(existente);

                    resultados.Add(new SyncResultDto
                    {
                        LocalId = dto.LocalId,
                        RemoteId = existente.Id,
                        Nombre = existente.Nombre,
                        Creado = false
                    });
                }
                else
                {
                    var nuevo = new Producto
                    {
                        CodigoBarras = dto.CodigoBarras,
                        Nombre = dto.Nombre,
                        Descripcion = dto.Descripcion,
                        CategoriaId = await ResolverCategoriaAsync(dto, categorias),
                        PrecioCompra = dto.PrecioCompra,
                        PrecioVenta = dto.PrecioVenta,
                        Stock = dto.Stock,
                        StockMinimo = dto.StockMinimo,
                        ImagenUrl = dto.ImagenUrl,
                        Orden = dto.Orden,
                    };

                    var id = await _productoRepo.CreateAsync(nuevo);

                    resultados.Add(new SyncResultDto
                    {
                        LocalId = dto.LocalId,
                        RemoteId = id,
                        Nombre = dto.Nombre,
                        Creado = true
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sincronizando producto {Producto}", dto.Nombre);
            }
        }

        return Ok(resultados);
    }

    [HttpGet("sync-pos")]
    public async Task<IActionResult> SyncToPOS([FromQuery] DateTime? desde)
    {
        var productos = desde.HasValue
            ? await _productoRepo.GetModificadosDesdeAsync(desde.Value)
            : await _productoRepo.GetAllAsync();

        var variantes = await _varianteRepo.GetAllAsync();
        var variantesLookup = variantes.ToLookup(v => v.ProductoId);

        return Ok(productos.Select(p => new SyncProductoDto
        {
            LocalId = p.Id,
            CodigoBarras = p.CodigoBarras,
            Nombre = p.Nombre,
            Descripcion = p.Descripcion,
            CategoriaId = p.CategoriaId,
            NombreCategoria = p.NombreCategoria,
            PrecioCompra = p.PrecioCompra,
            PrecioVenta = p.PrecioVenta,
            Stock = p.Stock,
            StockMinimo = p.StockMinimo,
            ImagenUrl = p.ImagenUrl,
            Orden = p.Orden,
            Activo = p.Activo,
            FechaModificacion = p.FechaModificacion ?? p.FechaCreacion,
            Variantes = variantesLookup[p.Id].Select(v => new SyncProductoVariantDto
            {
                Id = v.Id,
                ProductoId = v.ProductoId,
                Nombre = v.Nombre,
                ColorHex = v.ColorHex,
                Talla = v.Talla,
                Stock = v.Stock,
                ImagenUrl = v.ImagenUrl,
                Activo = v.Activo,
                Orden = v.Orden
            }).ToList()
        }).ToList());
    }

    [HttpPost("cleanup-duplicates")]
    public async Task<IActionResult> CleanupDuplicates()
    {
        _logger.LogInformation("Iniciando limpieza de productos duplicados");

        var (deleted, duplicates) = await _productoRepo.CleanupDuplicateNamesAsync();

        _logger.LogInformation("Limpieza completada: {Deleted} productos eliminados", deleted);

        return Ok(new
        {
            mensaje = $"Limpieza completada: {deleted} productos duplicados eliminados",
            eliminados = deleted,
            duplicadosEncontrados = duplicates
        });
    }

    [HttpDelete("por-nombre/{nombre}")]
    public async Task<IActionResult> DeleteByNombre(string nombre)
    {
        var eliminados = await _productoRepo.DeleteByNombreAsync(nombre);
        return Ok(new { mensaje = $"Productos '{nombre}' eliminados: {eliminados}", eliminados });
    }

    private async Task<int> ResolverCategoriaAsync(SyncProductoDto dto, List<Categoria> categorias)
    {
        if (categorias.Any(c => c.Id == dto.CategoriaId))
            return dto.CategoriaId;

        var porNombre = categorias.FirstOrDefault(c =>
            c.Nombre.Equals(dto.NombreCategoria, StringComparison.OrdinalIgnoreCase));
        if (porNombre != null)
            return porNombre.Id;

        var nueva = await _categoriaRepo.CreateAsync(new Categoria
        {
            Nombre = string.IsNullOrEmpty(dto.NombreCategoria) ? "General" : dto.NombreCategoria,
            Descripcion = "Sincronizada desde POS"
        });
        categorias.Add(new Categoria { Id = nueva, Nombre = dto.NombreCategoria });
        return nueva;
    }
}

public class CreateVarianteDto
{
    public string Nombre { get; set; } = string.Empty;
    public string ColorHex { get; set; } = "#000000";
    public string? Talla { get; set; }
    public int? Stock { get; set; }
    public string? ImagenUrl { get; set; }
    public int Orden { get; set; } = 0;
}

public class UpdateVarianteDto
{
    public string Nombre { get; set; } = string.Empty;
    public string ColorHex { get; set; } = "#000000";
    public string? Talla { get; set; }
    public int? Stock { get; set; }
    public string? ImagenUrl { get; set; }
    public bool Activo { get; set; } = true;
    public int Orden { get; set; } = 0;
}
