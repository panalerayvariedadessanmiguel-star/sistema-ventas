using Dapper;
using SistemaVentas.WebAPI.Data;

namespace SistemaVentas.WebAPI.Repositories;

public class StockRepository
{
    private readonly DbConnection _db;

    public StockRepository(DbConnection db)
    {
        _db = db;
    }

    public async Task<int> EjecutarTraspasoMasivoAsync(int añoAnterior, int mesAnterior, int añoNuevo, int mesNuevo)
    {
        using var conn = _db.GetConnection();
        var sql = @"
            INSERT INTO Stock (ProductoId, Anio, Mes, CantidadInicial, CantidadEntrante, CantidadSaliente, CantidadFinal, FechaRegistro)
            SELECT s.ProductoId, @AñoNuevo, @MesNuevo, s.CantidadFinal, 0, 0, s.CantidadFinal, NOW()
            FROM Stock s
            WHERE s.Anio = @AñoAnterior AND s.Mes = @MesAnterior
              AND NOT EXISTS (
                  SELECT 1 FROM Stock s2 
                  WHERE s2.ProductoId = s.ProductoId AND s2.Anio = @AñoNuevo AND s2.Mes = @MesNuevo
              )";
        return await conn.ExecuteAsync(sql, new { AñoAnterior = añoAnterior, MesAnterior = mesAnterior, AñoNuevo = añoNuevo, MesNuevo = mesNuevo });
    }

    public async Task<List<dynamic>> GetStockMesAsync(int año, int mes)
    {
        using var conn = _db.GetConnection();
        var sql = @"SELECT s.*, p.Nombre AS NombreProducto, p.CodigoBarras 
                    FROM Stock s 
                    INNER JOIN Productos p ON s.ProductoId = p.Id 
                    WHERE s.Anio = @Año AND s.Mes = @Mes 
                    ORDER BY p.Nombre";
        return (await conn.QueryAsync(sql, new { Año = año, Mes = mes })).AsList();
    }

    public async Task<bool> ExisteStockParaMesAsync(int año, int mes)
    {
        using var conn = _db.GetConnection();
        var sql = "SELECT COUNT(1) FROM Stock WHERE Anio = @Año AND Mes = @Mes";
        return await conn.ExecuteScalarAsync<int>(sql, new { Año = año, Mes = mes }) > 0;
    }
}
