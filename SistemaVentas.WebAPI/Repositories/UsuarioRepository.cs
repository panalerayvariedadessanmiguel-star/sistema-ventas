using Dapper;
using SistemaVentas.WebAPI.Data;
using SistemaVentas.WebAPI.Models;

namespace SistemaVentas.WebAPI.Repositories;

public class UsuarioRepository
{
    private readonly DbConnection _db;

    public UsuarioRepository(DbConnection db)
    {
        _db = db;
    }

    public async Task<Usuario?> LoginAsync(string documento, string contrasena)
    {
        using var conn = _db.GetConnection();
        return await conn.QueryFirstOrDefaultAsync<Usuario>(
            "SELECT * FROM Usuarios WHERE Documento = @Documento AND Contraseña = @Contrasena AND Activo = TRUE",
            new { Documento = documento, Contrasena = contrasena });
    }

    public async Task<bool> UpdatePasswordAsync(int id, string nuevaContrasena)
    {
        using var conn = _db.GetConnection();
        var rows = await conn.ExecuteAsync(
            "UPDATE Usuarios SET Contrasena = @Contrasena WHERE Id = @Id",
            new { Id = id, Contrasena = nuevaContrasena });
        return rows > 0;
    }
}
