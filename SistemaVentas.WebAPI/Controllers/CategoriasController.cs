using Microsoft.AspNetCore.Mvc;
using SistemaVentas.WebAPI.DTOs;
using SistemaVentas.WebAPI.Helpers;
using SistemaVentas.WebAPI.Models;
using SistemaVentas.WebAPI.Repositories;

namespace SistemaVentas.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriasController : ControllerBase
{
    private readonly CategoriaRepository _repo;

    public CategoriasController(CategoriaRepository repo)
    {
        _repo = repo;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool incluirInactivos = false)
    {
        var categorias = await _repo.GetAllAsync(incluirInactivos);
        return Ok(categorias);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCategoriaDto dto, [FromHeader] string? xAdminToken)
    {
        var userId = AdminAuth.GetUserId(xAdminToken);
        if (userId == null) return Unauthorized(new { mensaje = "Se requiere autenticacion de administrador" });

        var categoria = new Categoria
        {
            Nombre = dto.Nombre,
            Descripcion = dto.Descripcion
        };

        var id = await _repo.CreateAsync(categoria);
        categoria.Id = id;
        categoria.FechaCreacion = DateTime.Now;
        categoria.Activo = true;
        return CreatedAtAction(nameof(GetAll), new { id }, categoria);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCategoriaDto dto, [FromHeader] string? xAdminToken)
    {
        var userId = AdminAuth.GetUserId(xAdminToken);
        if (userId == null) return Unauthorized(new { mensaje = "Se requiere autenticacion de administrador" });

        var categoria = new Categoria
        {
            Id = id,
            Nombre = dto.Nombre,
            Descripcion = dto.Descripcion,
            Activo = dto.Activo
        };

        var result = await _repo.UpdateAsync(categoria);
        if (!result) return NotFound();
        return Ok(categoria);
    }
}
