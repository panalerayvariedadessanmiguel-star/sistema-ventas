using Dapper;
using SistemaVentas.WebAPI.Data;
using SistemaVentas.WebAPI.Models;

namespace SistemaVentas.WebAPI.Repositories;

public class VentaRepository
{
    private readonly DbConnection _db;

    public VentaRepository(DbConnection db)
    {
        _db = db;
    }

    public async Task<int> InsertWithDetailsAsync(Venta venta, List<DetalleVenta> detalles)
    {
        using var conn = _db.GetConnection();
        conn.Open();
        using var transaction = conn.BeginTransaction();

        try
        {
            var sqlVenta = @"INSERT INTO Ventas (NumeroVenta, CajaId, ClienteId, FechaVenta, SubTotal, Impuesto, Total, 
                            MetodoPago, MontoPagado, Cambio, Usuario, Origen, Estado, Domicilio) 
                            VALUES (@NumeroVenta, @CajaId, @ClienteId, @FechaVenta, @SubTotal, @Impuesto, @Total, 
                            @MetodoPago, @MontoPagado, @Cambio, @Usuario, @Origen, @Estado, @Domicilio)
                            RETURNING Id";
            var ventaId = await conn.ExecuteScalarAsync<int>(sqlVenta, new
            {
                venta.NumeroVenta,
                venta.CajaId,
                venta.ClienteId,
                venta.FechaVenta,
                venta.SubTotal,
                venta.Impuesto,
                venta.Total,
                venta.MetodoPago,
                venta.MontoPagado,
                venta.Cambio,
                venta.Usuario,
                venta.Origen,
                venta.Estado,
                venta.Domicilio
            }, transaction);

            var sqlDetalle = @"INSERT INTO DetalleVentas (VentaId, ProductoId, ProductoVarianteId, Cantidad, PrecioUnitario, CostoUnitario, SubTotal, Impuesto, Total) 
                               VALUES (@VentaId, @ProductoId, @ProductoVarianteId, @Cantidad, @PrecioUnitario, @CostoUnitario, @SubTotal, @Impuesto, @Total)";
            await conn.ExecuteAsync(sqlDetalle, detalles.Select(d => new
            {
                VentaId = ventaId,
                d.ProductoId,
                ProductoVarianteId = d.ProductoVarianteId,
                d.Cantidad,
                d.PrecioUnitario,
                d.CostoUnitario,
                d.SubTotal,
                d.Impuesto,
                d.Total
            }), transaction);

            // Solo descontar stock si la venta esta confirmada (no pendiente)
            if (venta.Estado == "Confirmada")
            {
                var sqlStock = "UPDATE Productos SET Stock = Stock - @Cantidad, FechaModificacion = NOW() WHERE Id = @Id";
                await conn.ExecuteAsync(sqlStock, detalles.Select(d => new { Id = d.ProductoId, d.Cantidad }), transaction);

                var sqlVarStock = "UPDATE ProductoVariantes SET Stock = Stock - @Cantidad WHERE Id = @Id AND Stock IS NOT NULL";
                var varUpdates = detalles.Where(d => d.ProductoVarianteId.HasValue)
                    .Select(d => new { Id = d.ProductoVarianteId.Value, d.Cantidad }).ToList();
                if (varUpdates.Count > 0)
                    await conn.ExecuteAsync(sqlVarStock, varUpdates, transaction);
            }

            transaction.Commit();
            return ventaId;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public async Task<bool> ConfirmarPagoAsync(int ventaId)
    {
        using var conn = _db.GetConnection();
        conn.Open();
        using var transaction = conn.BeginTransaction();

        try
        {
            var venta = await conn.QueryFirstOrDefaultAsync<Venta>(
                "SELECT * FROM Ventas WHERE Id = @Id", new { Id = ventaId }, transaction);
            if (venta == null || venta.Estado != "Pendiente")
                return false;

            var detalles = await conn.QueryAsync<DetalleVenta>(
                "SELECT * FROM DetalleVentas WHERE VentaId = @VentaId",
                new { VentaId = ventaId }, transaction);

            await conn.ExecuteAsync(
                "UPDATE Ventas SET Estado = 'Confirmada' WHERE Id = @Id",
                new { Id = ventaId }, transaction);

            foreach (var det in detalles)
            {
                await conn.ExecuteAsync(
                    "UPDATE Productos SET Stock = Stock - @Cantidad, FechaModificacion = NOW() WHERE Id = @Id AND Stock >= @Cantidad",
                    new { Id = det.ProductoId, Cantidad = det.Cantidad }, transaction);

                if (det.ProductoVarianteId.HasValue)
                {
                    await conn.ExecuteAsync(
                        "UPDATE ProductoVariantes SET Stock = Stock - @Cantidad WHERE Id = @Id AND Stock IS NOT NULL",
                        new { Id = det.ProductoVarianteId.Value, Cantidad = det.Cantidad }, transaction);
                }

                await conn.ExecuteAsync(
                    "INSERT INTO POSSync (VentaId, ProductoId, Cantidad, ProductoVarianteId) VALUES (@VentaId, @ProductoId, @Cantidad, @ProductoVarianteId)",
                    new { VentaId = ventaId, det.ProductoId, det.Cantidad, ProductoVarianteId = det.ProductoVarianteId }, transaction);
            }

            transaction.Commit();
            return true;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public async Task<List<dynamic>> GetPendientesPOSAsync()
    {
        using var conn = _db.GetConnection();
        var sql = @"SELECT ps.*, p.Nombre AS ProductoNombre, v.NumeroVenta, v.Total, dv.CostoUnitario, dv.PrecioUnitario
                    FROM POSSync ps
                    INNER JOIN Productos p ON ps.ProductoId = p.Id
                    INNER JOIN Ventas v ON ps.VentaId = v.Id
                    INNER JOIN DetalleVentas dv ON ps.VentaId = dv.VentaId AND ps.ProductoId = dv.ProductoId
                    WHERE ps.Sincronizado = 0
                    ORDER BY ps.Fecha ASC";
        return (await conn.QueryAsync<dynamic>(sql)).AsList();
    }

    public async Task<bool> MarcarPOSSyncAsync(int syncId)
    {
        using var conn = _db.GetConnection();
        var rows = await conn.ExecuteAsync(
            "UPDATE POSSync SET Sincronizado = 1 WHERE Id = @Id",
            new { Id = syncId });
        return rows > 0;
    }

    public async Task<List<Venta>> GetPendientesAsync()
    {
        using var conn = _db.GetConnection();
        var sql = @"SELECT v.*, c.Nombre AS NombreCliente, c.Documento AS DocumentoCliente, c.Telefono AS TelefonoCliente, c.Direccion AS DireccionCliente FROM Ventas v 
                    LEFT JOIN Clientes c ON v.ClienteId = c.Id 
                    WHERE v.Estado = 'Pendiente' AND v.Anulada = 0
                    ORDER BY v.FechaVenta DESC";
        return (await conn.QueryAsync<Venta>(sql)).AsList();
    }

    public async Task<List<Venta>> GetAllAsync()
    {
        using var conn = _db.GetConnection();
        var sql = @"SELECT v.*, c.Nombre AS NombreCliente, c.Documento AS DocumentoCliente, c.Telefono AS TelefonoCliente, c.Direccion AS DireccionCliente FROM Ventas v 
                    LEFT JOIN Clientes c ON v.ClienteId = c.Id 
                    ORDER BY v.FechaVenta DESC";
        return (await conn.QueryAsync<Venta>(sql)).AsList();
    }

    public async Task<Venta?> GetByIdAsync(int id)
    {
        using var conn = _db.GetConnection();
        var sql = @"SELECT v.*, c.Nombre AS NombreCliente, c.Documento AS DocumentoCliente, c.Telefono AS TelefonoCliente, c.Direccion AS DireccionCliente FROM Ventas v 
                    LEFT JOIN Clientes c ON v.ClienteId = c.Id 
                    WHERE v.Id = @Id";
        return await conn.QueryFirstOrDefaultAsync<Venta>(sql, new { Id = id });
    }

    public async Task<List<DetalleVenta>> GetDetallesByVentaIdAsync(int ventaId)
    {
        using var conn = _db.GetConnection();
        var sql = @"SELECT dv.*, p.Nombre AS NombreProducto FROM DetalleVentas dv 
                    INNER JOIN Productos p ON dv.ProductoId = p.Id 
                    WHERE dv.VentaId = @VentaId";
        return (await conn.QueryAsync<DetalleVenta>(sql, new { VentaId = ventaId })).AsList();
    }

    public async Task<List<Venta>> GetByClienteIdAsync(int clienteId)
    {
        using var conn = _db.GetConnection();
        var sql = @"SELECT v.*, c.Nombre AS NombreCliente, c.Documento AS DocumentoCliente, c.Telefono AS TelefonoCliente, c.Direccion AS DireccionCliente FROM Ventas v 
                    LEFT JOIN Clientes c ON v.ClienteId = c.Id 
                    WHERE v.ClienteId = @ClienteId
                    ORDER BY v.FechaVenta DESC";
        return (await conn.QueryAsync<Venta>(sql, new { ClienteId = clienteId })).AsList();
    }

    public async Task<string> GenerarNumeroVentaAsync()
    {
        using var conn = _db.GetConnection();
        var anio = DateTime.Now.Year;
        var mes = DateTime.Now.Month;
        var count = await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM Ventas WHERE EXTRACT(YEAR FROM FechaVenta) = @Anio AND EXTRACT(MONTH FROM FechaVenta) = @Mes",
            new { Anio = anio, Mes = mes });
            return $"VTA-{anio}{mes:D2}-{(count + 1):D6}";
    }

    public async Task<List<Venta>> GetVentasSincronizacionAsync(DateTime ultimaSync)
    {
        using var conn = _db.GetConnection();
        var sql = @"SELECT v.*, c.Nombre AS NombreCliente, c.Documento AS DocumentoCliente, c.Telefono AS TelefonoCliente, c.Direccion AS DireccionCliente FROM Ventas v 
                    LEFT JOIN Clientes c ON v.ClienteId = c.Id 
                    WHERE v.FechaVenta > @UltimaSync
                    ORDER BY v.FechaVenta ASC";
        return (await conn.QueryAsync<Venta>(sql, new { UltimaSync = ultimaSync })).AsList();
    }

    public async Task<bool> UpdateNumeroVentaAsync(int ventaId, string numeroVenta)
    {
        using var conn = _db.GetConnection();
        var sql = "UPDATE Ventas SET NumeroVenta = @NumeroVenta WHERE Id = @Id";
        var rows = await conn.ExecuteAsync(sql, new { Id = ventaId, NumeroVenta = numeroVenta });
        return rows > 0;
    }

    public async Task<bool> UpdateOrigenAsync(int ventaId, string origen)
    {
        using var conn = _db.GetConnection();
        var sql = "UPDATE Ventas SET Origen = @Origen WHERE Id = @Id";
        var rows = await conn.ExecuteAsync(sql, new { Id = ventaId, Origen = origen });
        return rows > 0;
    }

    public async Task<bool> AnularAsync(int ventaId, string motivo)
    {
        using var conn = _db.GetConnection();
        conn.Open();
        using var transaction = conn.BeginTransaction();

        try
        {
            var venta = await conn.QueryFirstOrDefaultAsync<Venta>(
                "SELECT * FROM Ventas WHERE Id = @Id", new { Id = ventaId }, transaction);

            var detalles = await conn.QueryAsync<DetalleVenta>(
                "SELECT * FROM DetalleVentas WHERE VentaId = @VentaId",
                new { VentaId = ventaId }, transaction);

            await conn.ExecuteAsync(
                "UPDATE Ventas SET Anulada = 1, Estado = 'Cancelada', MotivoAnulacion = @Motivo WHERE Id = @Id",
                new { Id = ventaId, Motivo = motivo }, transaction);

            // Solo restaurar stock si la venta estaba confirmada (el stock se descontó)
            if (venta?.Estado == "Confirmada")
            {
                foreach (var det in detalles)
                {
                    await conn.ExecuteAsync(
                        "UPDATE Productos SET Stock = Stock + @Cantidad, FechaModificacion = NOW() WHERE Id = @Id",
                        new { Id = det.ProductoId, Cantidad = det.Cantidad }, transaction);

                    if (det.ProductoVarianteId.HasValue)
                    {
                        await conn.ExecuteAsync(
                            "UPDATE ProductoVariantes SET Stock = Stock + @Cantidad WHERE Id = @Id AND Stock IS NOT NULL",
                            new { Id = det.ProductoVarianteId.Value, Cantidad = det.Cantidad }, transaction);
                    }
                }
            }

            transaction.Commit();
            return true;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }
}
