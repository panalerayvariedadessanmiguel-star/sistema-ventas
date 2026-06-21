using Dapper;
using SistemaVentas.WebAPI.Data;
using SistemaVentas.WebAPI.Models;

namespace SistemaVentas.WebAPI.Repositories;

public class ProductoVarianteRepository
{
    private readonly DbConnection _db;

    public ProductoVarianteRepository(DbConnection db)
    {
        _db = db;
    }

    public async Task<List<ProductoVariante>> GetAllAsync()
    {
        using var conn = _db.GetConnection();
        return (await conn.QueryAsync<ProductoVariante>(
            "SELECT * FROM ProductoVariantes WHERE Activo = TRUE ORDER BY ProductoId, Orden")).AsList();
    }

    public async Task<List<ProductoVariante>> GetByProductoAsync(int productoId)
    {
        using var conn = _db.GetConnection();
        return (await conn.QueryAsync<ProductoVariante>(
            "SELECT * FROM ProductoVariantes WHERE ProductoId = @ProductoId AND Activo = TRUE ORDER BY Orden",
            new { ProductoId = productoId })).AsList();
    }

    public async Task<ProductoVariante?> GetByIdAsync(int id)
    {
        using var conn = _db.GetConnection();
        return await conn.QueryFirstOrDefaultAsync<ProductoVariante>(
            "SELECT * FROM ProductoVariantes WHERE Id = @Id", new { Id = id });
    }

    public async Task<int> CreateAsync(ProductoVariante variante)
    {
        using var conn = _db.GetConnection();
        var sql = @"INSERT INTO ProductoVariantes (ProductoId, Nombre, ColorHex, Talla, Stock, ImagenUrl, Activo, Orden)
                    VALUES (@ProductoId, @Nombre, @ColorHex, @Talla, @Stock, @ImagenUrl, 1, @Orden)
                    RETURNING Id";
        return await conn.ExecuteScalarAsync<int>(sql, new
        {
            variante.ProductoId,
            variante.Nombre,
            variante.ColorHex,
            variante.Talla,
            variante.Stock,
            variante.ImagenUrl,
            variante.Orden
        });
    }

    public async Task<bool> UpdateAsync(ProductoVariante variante)
    {
        using var conn = _db.GetConnection();
        var sql = @"UPDATE ProductoVariantes SET
                    Nombre = @Nombre, ColorHex = @ColorHex, Talla = @Talla,
                    Stock = @Stock, ImagenUrl = @ImagenUrl, Activo = @Activo, Orden = @Orden
                    WHERE Id = @Id";
        var rows = await conn.ExecuteAsync(sql, variante);
        return rows > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var conn = _db.GetConnection();
        var rows = await conn.ExecuteAsync(
            "UPDATE ProductoVariantes SET Activo = FALSE WHERE Id = @Id", new { Id = id });
        return rows > 0;
    }
}
