using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;
using SistemaVentas.Data.Models;

namespace SistemaVentas.Data.Repositories
{
    public class ConteoFisicoRepository
    {
        public int InsertConteo(ConteoFisico conteo)
        {
            using var connection = DbConnection.GetConnection();
            var sql = @"INSERT INTO ConteosFisicos (Usuario, Observaciones, TipoConteo, ConteoOriginalId) 
                        VALUES (@Usuario, @Observaciones, @TipoConteo, @ConteoOriginalId);
                        SELECT SCOPE_IDENTITY();";
            return connection.ExecuteScalar<int>(sql, new { conteo.Usuario, conteo.Observaciones, conteo.TipoConteo, conteo.ConteoOriginalId });
        }

        public void InsertDetalle(DetalleConteoFisico detalle)
        {
            using var connection = DbConnection.GetConnection();
            var sql = @"INSERT INTO DetalleConteoFisico (ConteoId, ProductoId, StockSistema, StockFisico, ValorFaltante, ValorSobrante) 
                        VALUES (@ConteoId, @ProductoId, @StockSistema, @StockFisico, @ValorFaltante, @ValorSobrante)";
            connection.Execute(sql, detalle);
        }

        public IEnumerable<DetalleConteoFisico> GetDetallesByConteoId(int conteoId)
        {
            using var connection = DbConnection.GetConnection();
            var sql = @"SELECT d.*, p.Nombre AS NombreProducto, p.CodigoBarras, p.PrecioCompra 
                        FROM DetalleConteoFisico d
                        INNER JOIN Productos p ON d.ProductoId = p.Id
                        WHERE d.ConteoId = @ConteoId";
            return connection.Query<DetalleConteoFisico>(sql, new { ConteoId = conteoId });
        }

        public IEnumerable<ConteoFisico> GetAll()
        {
            using var connection = DbConnection.GetConnection();
            return connection.Query<ConteoFisico>(
                @"SELECT c.*, 
                         CASE WHEN c.ConteoOriginalId IS NOT NULL THEN CONCAT('(C2 desde C', c.ConteoOriginalId, ')') ELSE '' END AS ConteoOriginalDesc
                  FROM ConteosFisicos c ORDER BY c.Fecha DESC");
        }

        public ConteoFisico GetConteoById(int id)
        {
            using var connection = DbConnection.GetConnection();
            var conteo = connection.QueryFirstOrDefault<ConteoFisico>(
                @"SELECT c.*,
                         CASE WHEN c.ConteoOriginalId IS NOT NULL THEN CONCAT('(C2 desde C', c.ConteoOriginalId, ')') ELSE '' END AS ConteoOriginalDesc
                  FROM ConteosFisicos c WHERE c.Id = @Id", new { Id = id });
            if (conteo != null)
            {
                conteo.Detalles = GetDetallesByConteoId(id).ToList();
            }
            return conteo;
        }

        public IEnumerable<ConteoFisico> GetConteosFinalizadosTipo1()
        {
            using var connection = DbConnection.GetConnection();
            return connection.Query<ConteoFisico>(
                @"SELECT c.*, '' AS ConteoOriginalDesc 
                  FROM ConteosFisicos c 
                  WHERE c.Estado = 'Finalizado' AND (c.TipoConteo IS NULL OR c.TipoConteo = 1) 
                  ORDER BY c.Fecha DESC");
        }

        public IEnumerable<DetalleConteoFisico> GetDetallesConDiferencia(int conteoId)
        {
            using var connection = DbConnection.GetConnection();
            var sql = @"SELECT d.*, p.Nombre AS NombreProducto, p.CodigoBarras, p.PrecioCompra 
                        FROM DetalleConteoFisico d
                        INNER JOIN Productos p ON d.ProductoId = p.Id
                        WHERE d.ConteoId = @ConteoId AND d.Diferencia != 0";
            return connection.Query<DetalleConteoFisico>(sql, new { ConteoId = conteoId });
        }

        public bool FinalizarConteo(int conteoId)
        {
            using var connection = DbConnection.GetConnection();
            
            var sqlCalcular = @"
                SELECT 
                    ISNULL(SUM(CASE WHEN d.Diferencia < 0 THEN p.PrecioCompra * ABS(d.Diferencia) ELSE 0 END), 0) AS ValorFaltante,
                    ISNULL(SUM(CASE WHEN d.Diferencia > 0 THEN p.PrecioCompra * d.Diferencia ELSE 0 END), 0) AS ValorSobrante
                FROM DetalleConteoFisico d
                INNER JOIN Productos p ON d.ProductoId = p.Id
                WHERE d.ConteoId = @ConteoId";
            
            var valores = connection.QueryFirstOrDefault(sqlCalcular, new { ConteoId = conteoId });
            
            var sqlUpdate = @"UPDATE ConteosFisicos 
                             SET Estado = 'Finalizado', 
                                 ValorFaltante = @ValorFaltante, 
                                 ValorSobrante = @ValorSobrante 
                             WHERE Id = @Id";
            
            return connection.Execute(sqlUpdate, new { 
                ValorFaltante = valores.ValorFaltante, 
                ValorSobrante = valores.ValorSobrante, 
                Id = conteoId 
            }) > 0;
        }
    }
}
