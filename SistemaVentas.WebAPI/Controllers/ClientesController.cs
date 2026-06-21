using Microsoft.AspNetCore.Mvc;
using SistemaVentas.WebAPI.DTOs;
using SistemaVentas.WebAPI.Models;
using SistemaVentas.WebAPI.Repositories;
using SistemaVentas.WebAPI.Helpers;

namespace SistemaVentas.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClientesController : ControllerBase
{
    private readonly ClienteRepository _repo;

    public ClientesController(ClienteRepository repo)
    {
        _repo = repo;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var clientes = await _repo.GetAllAsync();
        return Ok(clientes);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var cliente = await _repo.GetByIdAsync(id);
        if (cliente == null) return NotFound();
        return Ok(cliente);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Cliente cliente)
    {
        var id = await _repo.InsertAsync(cliente);
        cliente.Id = id;
        return CreatedAtAction(nameof(GetById), new { id }, cliente);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Cliente cliente)
    {
        var existing = await _repo.GetByIdAsync(id);
        if (existing == null) return NotFound();

        if (!string.IsNullOrEmpty(cliente.Nombre)) existing.Nombre = cliente.Nombre;
        if (!string.IsNullOrEmpty(cliente.Telefono)) existing.Telefono = cliente.Telefono;
        if (!string.IsNullOrEmpty(cliente.Email)) existing.Email = cliente.Email;
        if (!string.IsNullOrEmpty(cliente.Direccion)) existing.Direccion = cliente.Direccion;
        if (!string.IsNullOrEmpty(cliente.Documento)) existing.Documento = cliente.Documento;

        await _repo.UpdateAsync(id, existing);
        return Ok(existing);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest dto)
    {
        var cliente = await _repo.LoginAsync(dto.Documento, dto.Contrasena);
        if (cliente == null)
            return Unauthorized(new { mensaje = "Documento o contrasena incorrectos" });

        var token = AdminAuth.GenerateToken(cliente.Id, "Cliente");
        return Ok(new
        {
            cliente.Id,
            cliente.Documento,
            cliente.Nombre,
            cliente.Telefono,
            cliente.Email,
            cliente.Direccion,
            Token = token
        });
    }

    [HttpPost("registro")]
    public async Task<IActionResult> Registro([FromBody] Cliente cliente)
    {
        if (string.IsNullOrEmpty(cliente.Documento) || string.IsNullOrEmpty(cliente.Contrasena))
            return BadRequest(new { mensaje = "Documento y contrasena son requeridos" });

        var id = await _repo.InsertAsync(cliente);
        cliente.Id = id;

        var token = AdminAuth.GenerateToken(cliente.Id, "Cliente");
        return Ok(new
        {
            cliente.Id,
            cliente.Documento,
            cliente.Nombre,
            cliente.Telefono,
            cliente.Email,
            cliente.Direccion,
            Token = token
        });
    }

    [HttpPut("password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest dto, [FromHeader] string? xClienteToken)
    {
        var clienteId = AdminAuth.GetClienteId(xClienteToken);
        if (clienteId == null)
            return Unauthorized(new { mensaje = "Se requiere autenticacion de cliente" });

        var result = await _repo.UpdatePasswordAsync(clienteId.Value, dto.NuevaContrasena);
        if (!result) return NotFound();

        return Ok(new { mensaje = "Contrasena actualizada exitosamente" });
    }
}
