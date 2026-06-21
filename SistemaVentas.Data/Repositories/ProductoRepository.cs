using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;
using Microsoft.Data.SqlClient;
using SistemaVentas.Data.Models;

namespace SistemaVentas.Data.Repositories
{
    public class ProductoRepository
    {
        private readonly ProductoCodigoBarrasRepository _barcodeRepository;

        public ProductoRepository()
        {
            _barcodeRepository = new ProductoCodigoBarrasRepository();
        }

        public List<Producto> GetAll()
        {
            using var connection = DbConnection.GetConnection();
            var sql = @"SELECT p.*, c.Nombre AS NombreCategoria 
                        FROM Productos p 
                        LEFT JOIN Categorias c ON p.CategoriaId = c.Id 
                        WHERE p.Activo = 1 
                        ORDER BY p.Id";
            var productos = connection.Query<Producto>(sql).AsList();
            foreach (var p in productos)
            {
                p.CodigosBarras = _barcodeRepository.GetByProductoId(p.Id);
            }
            return productos;
        }

        public List<Producto> GetModificadosDesde(DateTime desde)
        {
            using var connection = DbConnection.GetConnection();
            var sql = @"SELECT p.*, c.Nombre AS NombreCategoria 
                        FROM Productos p 
                        LEFT JOIN Categorias c ON p.CategoriaId = c.Id 
                        WHERE p.FechaModificacion > @Desde OR p.FechaCreacion > @Desde
                        ORDER BY p.Id";
            var productos = connection.Query<Producto>(sql, new { Desde = desde }).AsList();
            foreach (var p in productos)
            {
                p.CodigosBarras = _barcodeRepository.GetByProductoId(p.Id);
            }
            return productos;
        }

        public Producto GetById(int id)
        {
            using var connection = DbConnection.GetConnection();
            var sql = @"SELECT p.*, c.Nombre AS NombreCategoria 
                        FROM Productos p 
                        LEFT JOIN Categorias c ON p.CategoriaId = c.Id 
                        WHERE p.Id = @Id";
            var producto = connection.QueryFirstOrDefault<Producto>(sql, new { Id = id });
            if (producto != null)
            {
                producto.CodigosBarras = _barcodeRepository.GetByProductoId(producto.Id);
            }
            return producto;
        }

        public Producto GetByCodigoBarras(string codigoBarras)
        {
            using var connection = DbConnection.GetConnection();
            var sql = @"SELECT p.*, c.Nombre AS NombreCategoria 
                        FROM Productos p 
                        LEFT JOIN Categorias c ON p.CategoriaId = c.Id 
                        WHERE p.CodigoBarras = @CodigoBarras AND p.Activo = 1";
            var producto = connection.QueryFirstOrDefault<Producto>(sql, new { CodigoBarras = codigoBarras });

            if (producto == null)
            {
                var barcodeRecord = _barcodeRepository.GetByCodigoBarras(codigoBarras);
                if (barcodeRecord != null)
                {
                    producto = GetById(barcodeRecord.ProductoId);
                }
            }
            else
            {
                producto.CodigosBarras = _barcodeRepository.GetByProductoId(producto.Id);
            }

            return producto;
        }

        public int Insert(Producto producto)
        {
            using var connection = DbConnection.GetConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                var sql = @"INSERT INTO Productos (CodigoBarras, Nombre, Descripcion, CategoriaId, PrecioCompra, PrecioVenta, Stock, StockMinimo, Activo, FechaModificacion) 
                            VALUES (@CodigoBarras, @Nombre, @Descripcion, @CategoriaId, @PrecioCompra, @PrecioVenta, @Stock, @StockMinimo, @Activo, GETDATE());
                            SELECT SCOPE_IDENTITY();";
                var productoId = connection.ExecuteScalar<int>(sql, producto, transaction);

                if (producto.CodigosBarras != null)
                {
                    foreach (var barcode in producto.CodigosBarras.Where(b => !string.IsNullOrWhiteSpace(b.CodigoBarras)))
                    {
                        var sqlBarcode = @"INSERT INTO ProductoCodigosBarras (ProductoId, CodigoBarras) VALUES (@ProductoId, @CodigoBarras)";
                        connection.Execute(sqlBarcode, new { ProductoId = productoId, barcode.CodigoBarras }, transaction);
                    }
                }

                transaction.Commit();
                return productoId;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public bool Update(Producto producto)
        {
            using var connection = new SqlConnection(DbConnection.GetConnection().ConnectionString);
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                var sql = @"UPDATE Productos SET CodigoBarras = @CodigoBarras, Nombre = @Nombre, Descripcion = @Descripcion, 
                            CategoriaId = @CategoriaId, PrecioCompra = @PrecioCompra, PrecioVenta = @PrecioVenta, 
                            Stock = @Stock, StockMinimo = @StockMinimo, Activo = @Activo, FechaModificacion = GETDATE()
                            WHERE Id = @Id";
                var result = connection.Execute(sql, producto, transaction) > 0;

                if (result)
                {
                    SincronizarStock(connection, producto.Id, producto.Stock, transaction);

                    var barcodesExistentes = connection.Query<string>(
                        "SELECT CodigoBarras FROM ProductoCodigosBarras WHERE ProductoId = @ProductoId",
                        new { ProductoId = producto.Id }, transaction).ToList();

                    var barcodesNuevos = producto.CodigosBarras?
                        .Where(b => !string.IsNullOrWhiteSpace(b.CodigoBarras))
                        .Select(b => b.CodigoBarras)
                        .ToList() ?? new List<string>();

                    var aEliminar = barcodesExistentes.Except(barcodesNuevos).ToList();
                    foreach (var codigo in aEliminar)
                    {
                        connection.Execute(
                            "DELETE FROM ProductoCodigosBarras WHERE ProductoId = @ProductoId AND CodigoBarras = @CodigoBarras",
                            new { ProductoId = producto.Id, CodigoBarras = codigo }, transaction);
                    }

                    var aInsertar = barcodesNuevos.Except(barcodesExistentes).ToList();
                    foreach (var codigo in aInsertar)
                    {
                        connection.Execute(
                            "INSERT INTO ProductoCodigosBarras (ProductoId, CodigoBarras) VALUES (@ProductoId, @CodigoBarras)",
                            new { ProductoId = producto.Id, CodigoBarras = codigo }, transaction);
                    }
                }

                transaction.Commit();
                return result;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public bool UpdateFromSync(Producto producto)
        {
            using var connection = new SqlConnection(DbConnection.GetConnection().ConnectionString);
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                var sql = @"UPDATE Productos SET CodigoBarras = @CodigoBarras, Nombre = @Nombre, Descripcion = @Descripcion, 
                            CategoriaId = @CategoriaId, PrecioCompra = @PrecioCompra, PrecioVenta = @PrecioVenta, 
                            Stock = @Stock, StockMinimo = @StockMinimo, Activo = @Activo
                            WHERE Id = @Id";
                var result = connection.Execute(sql, producto, transaction) > 0;

                if (result)
                {
                    var barcodesExistentes = connection.Query<string>(
                        "SELECT CodigoBarras FROM ProductoCodigosBarras WHERE ProductoId = @ProductoId",
                        new { ProductoId = producto.Id }, transaction).ToList();

                    var barcodesNuevos = producto.CodigosBarras?
                        .Where(b => !string.IsNullOrWhiteSpace(b.CodigoBarras))
                        .Select(b => b.CodigoBarras)
                        .ToList() ?? new List<string>();

                    var aEliminar = barcodesExistentes.Except(barcodesNuevos).ToList();
                    foreach (var codigo in aEliminar)
                    {
                        connection.Execute(
                            "DELETE FROM ProductoCodigosBarras WHERE ProductoId = @ProductoId AND CodigoBarras = @CodigoBarras",
                            new { ProductoId = producto.Id, CodigoBarras = codigo }, transaction);
                    }

                    var aInsertar = barcodesNuevos.Except(barcodesExistentes).ToList();
                    foreach (var codigo in aInsertar)
                    {
                        connection.Execute(
                            "INSERT INTO ProductoCodigosBarras (ProductoId, CodigoBarras) VALUES (@ProductoId, @CodigoBarras)",
                            new { ProductoId = producto.Id, CodigoBarras = codigo }, transaction);
                    }
                }

                transaction.Commit();
                return result;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        private void SincronizarStock(SqlConnection connection, int productoId, int stockActual, SqlTransaction transaction)
        {
            var año = DateTime.Now.Year;
            var mes = DateTime.Now.Month;

            var stockExistente = connection.QueryFirstOrDefault<Stock>(
                "SELECT * FROM Stock WHERE ProductoId = @ProductoId AND Año = @Año AND Mes = @Mes",
                new { ProductoId = productoId, Año = año, Mes = mes }, transaction);

            if (stockExistente == null)
            {
                connection.Execute(
                    "INSERT INTO Stock (ProductoId, Año, Mes, CantidadInicial, CantidadEntrante, CantidadSaliente, CantidadFinal) " +
                    "VALUES (@ProductoId, @Año, @Mes, @CantidadInicial, 0, 0, @CantidadFinal)",
                    new { ProductoId = productoId, Año = año, Mes = mes, CantidadInicial = stockActual, CantidadFinal = stockActual }, transaction);
            }
            else
            {
                connection.Execute(
                    "UPDATE Stock SET CantidadFinal = @CantidadFinal WHERE Id = @Id",
                    new { Id = stockExistente.Id, CantidadFinal = stockActual }, transaction);
            }
        }

        public bool Delete(int id)
        {
            using var connection = DbConnection.GetConnection();
            return connection.Execute("UPDATE Productos SET Activo = 0, FechaModificacion = GETDATE() WHERE Id = @Id", new { Id = id }) > 0;
        }

        public List<Producto> Search(string termino)
        {
            using var connection = DbConnection.GetConnection();
            var sql = @"SELECT DISTINCT p.*, c.Nombre AS NombreCategoria 
                        FROM Productos p 
                        LEFT JOIN Categorias c ON p.CategoriaId = c.Id 
                        LEFT JOIN ProductoCodigosBarras b ON p.Id = b.ProductoId
                        WHERE p.Activo = 1 AND (p.Nombre LIKE @Termino OR p.CodigoBarras LIKE @Termino OR b.CodigoBarras LIKE @Termino) 
                        ORDER BY p.Nombre";
            var productos = connection.Query<Producto>(sql, new { Termino = "%" + termino + "%" }).AsList();
            foreach (var p in productos)
            {
                p.CodigosBarras = _barcodeRepository.GetByProductoId(p.Id);
            }
            return productos;
        }

        public List<Producto> GetByCategoria(int categoriaId)
        {
            using var connection = DbConnection.GetConnection();
            var sql = @"SELECT p.*, c.Nombre AS NombreCategoria 
                        FROM Productos p 
                        LEFT JOIN Categorias c ON p.CategoriaId = c.Id 
                        WHERE p.Activo = 1 AND p.CategoriaId = @CategoriaId 
                        ORDER BY p.Nombre";
            var productos = connection.Query<Producto>(sql, new { CategoriaId = categoriaId }).AsList();
            foreach (var p in productos)
            {
                p.CodigosBarras = _barcodeRepository.GetByProductoId(p.Id);
            }
            return productos;
        }

        public Producto GetByNombre(string nombre)
        {
            using var connection = DbConnection.GetConnection();
            var sql = @"SELECT p.*, c.Nombre AS NombreCategoria 
                        FROM Productos p 
                        LEFT JOIN Categorias c ON p.CategoriaId = c.Id 
                        WHERE LOWER(p.Nombre) = LOWER(@Nombre) AND p.Activo = 1";
            var producto = connection.QueryFirstOrDefault<Producto>(sql, new { Nombre = nombre });
            if (producto != null)
            {
                producto.CodigosBarras = _barcodeRepository.GetByProductoId(producto.Id);
            }
            return producto;
        }

        public bool UpdateStock(int productoId, int cantidad)
        {
            using var connection = DbConnection.GetConnection();
            return connection.Execute("UPDATE Productos SET Stock = Stock - @Cantidad, FechaModificacion = GETDATE() WHERE Id = @Id",
                new { Id = productoId, Cantidad = cantidad }) > 0;
        }

        public bool ActualizarStock(int productoId, int cantidad, bool esEntrada)
        {
            using var connection = DbConnection.GetConnection();
            var sql = esEntrada 
                ? "UPDATE Productos SET Stock = Stock + @Cantidad, FechaModificacion = GETDATE() WHERE Id = @Id"
                : "UPDATE Productos SET Stock = Stock - @Cantidad, FechaModificacion = GETDATE() WHERE Id = @Id";
            return connection.Execute(sql, new { Id = productoId, Cantidad = cantidad }) > 0;
        }
    }
}
