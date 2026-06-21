using System;
using System.Collections.Generic;
using System.Data;
using Dapper;
using SistemaVentas.Data.Models;

namespace SistemaVentas.Data.Repositories
{
    public class ReporteRepository
    {
        public List<dynamic> GetUtilidadesMensuales(int anio, int mes)
        {
            using var connection = DbConnection.GetConnection();
            var sql = @"SELECT
                            p.Nombre AS Producto,
                            SUM(dv.Cantidad) AS CantidadVendida,
                            SUM(dv.SubTotal) AS TotalVentas,
                            SUM(dv.CostoUnitario * dv.Cantidad) AS TotalCosto,
                            SUM(dv.SubTotal - (dv.CostoUnitario * dv.Cantidad)) AS Utilidad
                        FROM DetalleVentas dv
                        INNER JOIN Productos p ON dv.ProductoId = p.Id
                        INNER JOIN Ventas v ON dv.VentaId = v.Id
                        WHERE YEAR(v.FechaVenta) = @Anio AND MONTH(v.FechaVenta) = @Mes AND v.Anulada = 0
                        GROUP BY p.Nombre
                        ORDER BY Utilidad DESC";
            return connection.Query(sql, new { Anio = anio, Mes = mes }).AsList();
        }

        public decimal GetTotalUtilidadMensual(int anio, int mes)
        {
            using var connection = DbConnection.GetConnection();
            var sql = @"SELECT COALESCE(SUM(dv.SubTotal - (dv.CostoUnitario * dv.Cantidad)), 0)
                        FROM DetalleVentas dv
                        INNER JOIN Ventas v ON dv.VentaId = v.Id
                        WHERE YEAR(v.FechaVenta) = @Anio AND MONTH(v.FechaVenta) = @Mes AND v.Anulada = 0";
            var result = connection.ExecuteScalar<decimal?>(sql, new { Anio = anio, Mes = mes });
            return result ?? 0;
        }

        public decimal GetTotalVentasMensual(int anio, int mes)
        {
            using var connection = DbConnection.GetConnection();
            var sql = @"SELECT COALESCE(SUM(dv.SubTotal), 0)
                        FROM DetalleVentas dv
                        INNER JOIN Ventas v ON dv.VentaId = v.Id
                        WHERE YEAR(v.FechaVenta) = @Anio AND MONTH(v.FechaVenta) = @Mes AND v.Anulada = 0";
            var result = connection.ExecuteScalar<decimal?>(sql, new { Anio = anio, Mes = mes });
            return result ?? 0;
        }

        public int GetTotalTransaccionesMensual(int anio, int mes)
        {
            using var connection = DbConnection.GetConnection();
            var sql = @"SELECT COUNT(*) FROM Ventas v
                        WHERE YEAR(v.FechaVenta) = @Anio AND MONTH(v.FechaVenta) = @Mes AND v.Anulada = 0";
            return connection.ExecuteScalar<int>(sql, new { Anio = anio, Mes = mes });
        }

        public List<dynamic> GetVentasDiarias(DateTime fecha)
        {
            using var connection = DbConnection.GetConnection();
            var sql = @"SELECT
                            v.Id,
                            v.NumeroVenta,
                            v.FechaVenta,
                            v.Total,
                            v.MetodoPago,
                            v.Usuario,
                            COALESCE(c.Nombre, 'Sin cliente') AS NombreCliente
                        FROM Ventas v
                        LEFT JOIN Clientes c ON v.ClienteId = c.Id
                        WHERE CAST(v.FechaVenta AS DATE) = @Fecha AND v.Anulada = 0
                        ORDER BY v.FechaVenta DESC";
            return connection.Query(sql, new { Fecha = fecha.Date }).AsList();
        }

        public List<dynamic> GetProductosVendidosDiarios(DateTime fecha)
        {
            using var connection = DbConnection.GetConnection();
            var sql = @"SELECT
                            p.CodigoBarras,
                            p.Nombre AS Producto,
                            SUM(dv.Cantidad) AS Cantidad,
                            dv.PrecioUnitario,
                            SUM(dv.SubTotal) AS SubTotal
                        FROM DetalleVentas dv
                        INNER JOIN Productos p ON dv.ProductoId = p.Id
                        INNER JOIN Ventas v ON dv.VentaId = v.Id
                        WHERE CAST(v.FechaVenta AS DATE) = @Fecha AND v.Anulada = 0
                        GROUP BY p.Id, p.CodigoBarras, p.Nombre, dv.PrecioUnitario
                        ORDER BY p.Nombre";
            return connection.Query(sql, new { Fecha = fecha.Date }).AsList();
        }

        public decimal GetTotalVentasDiarias(DateTime fecha)
        {
            using var connection = DbConnection.GetConnection();
            var sql = @"SELECT COALESCE(SUM(v.Total), 0)
                        FROM Ventas v
                        WHERE CAST(v.FechaVenta AS DATE) = @Fecha AND v.Anulada = 0";
            var result = connection.ExecuteScalar<decimal?>(sql, new { Fecha = fecha.Date });
            return result ?? 0;
        }

        public int GetTotalTransaccionesDiarias(DateTime fecha)
        {
            using var connection = DbConnection.GetConnection();
            var sql = @"SELECT COUNT(*) FROM Ventas v
                        WHERE CAST(v.FechaVenta AS DATE) = @Fecha AND v.Anulada = 0";
            return connection.ExecuteScalar<int>(sql, new { Fecha = fecha.Date });
        }

        public List<dynamic> GetUtilidadPorCategoriaDiaria(DateTime fecha)
        {
            using var connection = DbConnection.GetConnection();
            var sql = @"SELECT
                            c.Nombre AS Categoria,
                            SUM(dv.SubTotal - (dv.CostoUnitario * dv.Cantidad)) AS Utilidad,
                            SUM(dv.SubTotal) AS TotalVentas,
                            SUM(dv.Cantidad) AS CantidadVendida
                        FROM DetalleVentas dv
                        INNER JOIN Productos p ON dv.ProductoId = p.Id
                        INNER JOIN Categorias c ON p.CategoriaId = c.Id
                        INNER JOIN Ventas v ON dv.VentaId = v.Id
                        WHERE CAST(v.FechaVenta AS DATE) = @Fecha AND v.Anulada = 0
                        GROUP BY c.Nombre
                        ORDER BY Utilidad DESC";
            return connection.Query(sql, new { Fecha = fecha.Date }).AsList();
        }
    }
}
