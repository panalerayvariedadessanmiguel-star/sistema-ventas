using Dapper;
using SistemaVentas.WebAPI.Data;
using SistemaVentas.WebAPI.Models;

namespace SistemaVentas.WebAPI.Repositories;

public class CategoriaRepository
{
    private readonly DbConnection _db;

    public CategoriaRepository(DbConnection db)
    {
        _db = db;
    }

    public async Task<List<Categoria>> GetAllAsync(bool includeInactive = false)
    {
        using var conn = _db.GetConnection();
        var where = includeInactive ? "1 = 1" : "Activo = TRUE";
        return (await conn.QueryAsync<Categoria>(
            $"SELECT * FROM Categorias WHERE {where} ORDER BY Nombre")).AsList();
    }

    public async Task<int> CreateAsync(Categoria categoria)
    {
        using var conn = _db.GetConnection();
        var sql = @"INSERT INTO Categorias (Nombre, Descripcion, FechaCreacion, Activo)
                    VALUES (@Nombre, @Descripcion, NOW(), 1)
                    RETURNING Id";
        return await conn.ExecuteScalarAsync<int>(sql, new { categoria.Nombre, categoria.Descripcion });
    }

    public async Task<bool> UpdateAsync(Categoria categoria)
    {
        using var conn = _db.GetConnection();
        var sql = @"UPDATE Categorias SET Nombre = @Nombre, Descripcion = @Descripcion, Activo = @Activo WHERE Id = @Id";
        var rows = await conn.ExecuteAsync(sql, categoria);
        return rows > 0;
    }
}
