using Dapper;
using SistemaVentas.WebAPI.Data;
using SistemaVentas.WebAPI.Models;

namespace SistemaVentas.WebAPI.Repositories;

public class ClienteRepository
{
    private readonly DbConnection _db;

    public ClienteRepository(DbConnection db)
    {
        _db = db;
    }

    public async Task<List<Cliente>> GetAllAsync()
    {
        using var conn = _db.GetConnection();
        return (await conn.QueryAsync<Cliente>(
            "SELECT * FROM Clientes WHERE Activo = TRUE ORDER BY Nombre")).AsList();
    }

    public async Task<Cliente?> GetByIdAsync(int id)
    {
        using var conn = _db.GetConnection();
        return await conn.QueryFirstOrDefaultAsync<Cliente>(
            "SELECT * FROM Clientes WHERE Id = @Id", new { Id = id });
    }

    public async Task<int> InsertAsync(Cliente cliente)
    {
        using var conn = _db.GetConnection();
        var sql = @"INSERT INTO Clientes (Documento, Nombre, Telefono, Email, Direccion, Contrasena) 
                    VALUES (@Documento, @Nombre, @Telefono, @Email, @Direccion, @Contrasena)
                    RETURNING Id";
        return await conn.ExecuteScalarAsync<int>(sql, cliente);
    }

    public async Task UpdateAsync(int id, Cliente cliente)
    {
        using var conn = _db.GetConnection();
        var sql = @"UPDATE Clientes SET Documento=@Documento, Nombre=@Nombre, Telefono=@Telefono, 
                    Email=@Email, Direccion=@Direccion WHERE Id=@Id";
        await conn.ExecuteAsync(sql, new { cliente.Documento, cliente.Nombre, cliente.Telefono, cliente.Email, cliente.Direccion, Id = id });
    }

    public async Task<Cliente?> LoginAsync(string documento, string contrasena)
    {
        using var conn = _db.GetConnection();
        return await conn.QueryFirstOrDefaultAsync<Cliente>(
            "SELECT * FROM Clientes WHERE Documento = @Documento AND Contrasena = @Contrasena AND Activo = TRUE",
            new { Documento = documento, Contrasena = contrasena });
    }

    public async Task<bool> UpdatePasswordAsync(int id, string nuevaContrasena)
    {
        using var conn = _db.GetConnection();
        var rows = await conn.ExecuteAsync(
            "UPDATE Clientes SET Contrasena = @Contrasena WHERE Id = @Id AND Activo = TRUE",
            new { Id = id, Contrasena = nuevaContrasena });
        return rows > 0;
    }
}
