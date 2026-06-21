using Dapper;
using SistemaVentas.WebAPI.Data;
using SistemaVentas.WebAPI.Models;

namespace SistemaVentas.WebAPI.Repositories;

public class ProductoRepository
{
    private readonly DbConnection _db;

    public ProductoRepository(DbConnection db)
    {
        _db = db;
    }

    public async Task<List<Producto>> GetAllAsync()
    {
        using var conn = _db.GetConnection();
        var sql = @"SELECT p.*, c.Nombre AS NombreCategoria 
                    FROM Productos p 
                    LEFT JOIN Categorias c ON p.CategoriaId = c.Id 
                    WHERE p.Activo = TRUE 
                    ORDER BY p.Orden ASC, p.Nombre";
        return (await conn.QueryAsync<Producto>(sql)).AsList();
    }

    public async Task<List<Producto>> GetModificadosDesdeAsync(DateTime desde)
    {
        using var conn = _db.GetConnection();
        var sql = @"SELECT p.*, c.Nombre AS NombreCategoria 
                    FROM Productos p 
                    LEFT JOIN Categorias c ON p.CategoriaId = c.Id 
                    WHERE (p.FechaCreacion >= @Desde OR p.FechaModificacion >= @Desde)
                    ORDER BY p.Orden ASC, p.Nombre";
        return (await conn.QueryAsync<Producto>(sql, new { Desde = desde })).AsList();
    }

    public async Task<Producto?> GetByIdAsync(int id)
    {
        using var conn = _db.GetConnection();
        var sql = @"SELECT p.*, c.Nombre AS NombreCategoria 
                    FROM Productos p 
                    LEFT JOIN Categorias c ON p.CategoriaId = c.Id 
                    WHERE p.Id = @Id";
        return await conn.QueryFirstOrDefaultAsync<Producto>(sql, new { Id = id });
    }

    public async Task<List<Producto>> GetByCategoriaAsync(int categoriaId)
    {
        using var conn = _db.GetConnection();
        var sql = @"SELECT p.*, c.Nombre AS NombreCategoria 
                    FROM Productos p 
                    LEFT JOIN Categorias c ON p.CategoriaId = c.Id 
                    WHERE p.Activo = TRUE AND p.CategoriaId = @CategoriaId 
                    ORDER BY p.Orden ASC, p.Nombre";
        return (await conn.QueryAsync<Producto>(sql, new { CategoriaId = categoriaId })).AsList();
    }

    public async Task<List<Producto>> SearchAsync(string term)
    {
        using var conn = _db.GetConnection();
        var sql = @"SELECT p.*, c.Nombre AS NombreCategoria 
                    FROM Productos p 
                    LEFT JOIN Categorias c ON p.CategoriaId = c.Id 
                    WHERE p.Activo = TRUE AND (p.Nombre ILIKE @Term OR p.Descripcion ILIKE @Term)
                    ORDER BY p.Nombre
                    LIMIT 20";
        return (await conn.QueryAsync<Producto>(sql, new { Term = $"%{term}%" })).AsList();
    }

    public async Task<int> CreateAsync(Producto producto)
    {
        using var conn = _db.GetConnection();
        var sql = @"INSERT INTO Productos (CodigoBarras, Nombre, Descripcion, CategoriaId, PrecioCompra, PrecioVenta, Stock, StockMinimo, ImagenUrl, Orden, FechaCreacion, FechaModificacion, Activo)
                    VALUES (@CodigoBarras, @Nombre, @Descripcion, @CategoriaId, @PrecioCompra, @PrecioVenta, @Stock, @StockMinimo, @ImagenUrl, @Orden, NOW(), NOW(), TRUE)
                    RETURNING Id";
        return await conn.ExecuteScalarAsync<int>(sql, new { producto.CodigoBarras, producto.Nombre, producto.Descripcion, producto.CategoriaId, producto.PrecioCompra, producto.PrecioVenta, producto.Stock, producto.StockMinimo, producto.ImagenUrl, producto.Orden });
    }

    public async Task<bool> UpdateAsync(Producto producto)
    {
        using var conn = _db.GetConnection();
        var sql = @"UPDATE Productos SET CodigoBarras = @CodigoBarras, Nombre = @Nombre, Descripcion = @Descripcion,
                    CategoriaId = @CategoriaId, PrecioCompra = @PrecioCompra, PrecioVenta = @PrecioVenta,
                    Stock = @Stock, StockMinimo = @StockMinimo, ImagenUrl = @ImagenUrl, Orden = @Orden, Activo = @Activo,
                    FechaModificacion = NOW()
                    WHERE Id = @Id";
        var rows = await conn.ExecuteAsync(sql, producto);
        return rows > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var conn = _db.GetConnection();
        var sql = "UPDATE Productos SET Activo = FALSE, FechaModificacion = NOW() WHERE Id = @Id";
        var rows = await conn.ExecuteAsync(sql, new { Id = id });
        return rows > 0;
    }

    public async Task<bool> UpdateStockAsync(int id, int cantidad)
    {
        using var conn = _db.GetConnection();
        var sql = "UPDATE Productos SET Stock = Stock + @Cantidad, FechaModificacion = NOW() WHERE Id = @Id";
        var rows = await conn.ExecuteAsync(sql, new { Id = id, Cantidad = cantidad });
        return rows > 0;
    }

    public async Task<Producto?> GetByCodigoBarrasAsync(string codigoBarras)
        {
            using var conn = _db.GetConnection();
            var sql = @"SELECT p.*, c.Nombre AS NombreCategoria 
                        FROM Productos p 
                        LEFT JOIN Categorias c ON p.CategoriaId = c.Id 
                        WHERE p.CodigoBarras = @CodigoBarras";
            return await conn.QueryFirstOrDefaultAsync<Producto>(sql, new { CodigoBarras = codigoBarras });
        }

    public async Task<Producto?> GetByNombreAsync(string nombre)
    {
        using var conn = _db.GetConnection();
        var sql = @"SELECT p.*, c.Nombre AS NombreCategoria 
                    FROM Productos p 
                    LEFT JOIN Categorias c ON p.CategoriaId = c.Id 
                    WHERE LOWER(p.Nombre) = LOWER(@Nombre) AND p.Activo = TRUE
                    LIMIT 1";
        return await conn.QueryFirstOrDefaultAsync<Producto>(sql, new { Nombre = nombre });
    }

    public async Task<(int Deleted, List<string> Duplicates)> CleanupDuplicateNamesAsync()
    {
        using var conn = _db.GetConnection();

        var previewSql = @"SELECT Nombre, COUNT(*) AS Cnt, MIN(Id) AS KeepId
                           FROM Productos
                           WHERE (CodigoBarras IS NULL OR CodigoBarras = '')
                             AND Activo = TRUE
                           GROUP BY Nombre
                           HAVING COUNT(*) > 1
                           ORDER BY Nombre";
        var duplicates = (await conn.QueryAsync<(string Nombre, int Cnt, int KeepId)>(previewSql))
            .Select(d => $"{d.Nombre} x{d.Cnt}")
            .ToList();

        var deleteSql = @"DELETE FROM Productos
                          USING (
                              SELECT MIN(Id) AS KeepId, Nombre
                              FROM Productos
                              WHERE (CodigoBarras IS NULL OR CodigoBarras = '')
                                AND Activo = TRUE
                              GROUP BY Nombre
                              HAVING COUNT(*) > 1
                          ) d
                          WHERE Productos.Nombre = d.Nombre
                            AND Productos.Id != d.KeepId
                            AND (Productos.CodigoBarras IS NULL OR Productos.CodigoBarras = '')";
        var deleted = await conn.ExecuteAsync(deleteSql);

        return (deleted, duplicates);
    }

    public async Task<int> DeleteByNombreAsync(string nombre)
    {
        using var conn = _db.GetConnection();
        var sql = "UPDATE Productos SET Activo = FALSE WHERE LOWER(Nombre) = LOWER(@Nombre) AND Activo = TRUE";
        return await conn.ExecuteAsync(sql, new { Nombre = nombre });
    }
}
