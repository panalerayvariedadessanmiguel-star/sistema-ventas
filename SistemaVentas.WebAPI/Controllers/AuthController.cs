using Microsoft.AspNetCore.Mvc;
using SistemaVentas.WebAPI.DTOs;
using SistemaVentas.WebAPI.Helpers;
using SistemaVentas.WebAPI.Repositories;

namespace SistemaVentas.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UsuarioRepository _repo;

    public AuthController(UsuarioRepository repo)
    {
        _repo = repo;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var usuario = await _repo.LoginAsync(dto.Documento, dto.Contrasena);
        if (usuario == null)
            return Unauthorized(new { mensaje = "Credenciales invalidas" });

        var token = AdminAuth.GenerateToken(usuario.Id, usuario.Rol);

        return Ok(new
        {
            usuario.Id,
            usuario.Nombres,
            usuario.Apellidos,
            usuario.Documento,
            usuario.Rol,
            Token = token
        });
    }

    [HttpPut("password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto, [FromHeader] string? xAdminToken)
    {
        var userId = AdminAuth.GetUserId(xAdminToken);
        if (userId == null) return Unauthorized(new { mensaje = "Se requiere autenticacion de administrador" });

        var result = await _repo.UpdatePasswordAsync(userId.Value, dto.NuevaContrasena);
        if (!result) return NotFound();

        return Ok(new { mensaje = "Contrasena actualizada exitosamente" });
    }
}
