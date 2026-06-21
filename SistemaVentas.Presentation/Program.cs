using System;
using System.Data;
using System.Windows.Forms;
using System.Globalization;
using Microsoft.Data.SqlClient;
using Dapper;
using SistemaVentas.Presentation;

namespace SistemaVentas.Presentation
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            try
            {
                var cultura = new CultureInfo("es-CO");
                CultureInfo.CurrentCulture = cultura;
                CultureInfo.CurrentUICulture = cultura;

                CrearBaseDatosSiNoExiste();

                ApplicationConfiguration.Initialize();
                Application.Run(new FormPrincipal());
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        static void CrearBaseDatosSiNoExiste()
        {
            try
            {
                string connectionStringMaster = "Server=(LocalDB)\\MSSQLLocalDB;Database=master;Integrated Security=true;TrustServerCertificate=true;";
                
                using (var connection = new SqlConnection(connectionStringMaster))
                {
                    connection.Open();
                    
                    // Verificar si la base de datos existe
                    var existeDB = connection.QueryFirstOrDefault<int>(
                        "SELECT COUNT(*) FROM sys.databases WHERE name = 'SistemaVentasDB'");

                    if (existeDB == 0)
                    {
                        // Crear base de datos
                        connection.Execute("CREATE DATABASE SistemaVentasDB");
                    }
                }

                // Ahora crear las tablas si no existen
                string connectionStringDB = "Server=(LocalDB)\\MSSQLLocalDB;Database=SistemaVentasDB;Integrated Security=true;TrustServerCertificate=true;";
                
                using (var connection = new SqlConnection(connectionStringDB))
                {
                    connection.Open();

                    // Crear tablas una por una
                    var tablas = new string[]
                    {
                        @"IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Categorias]') AND type in (N'U'))
                        CREATE TABLE Categorias (
                            Id INT IDENTITY(1,1) PRIMARY KEY,
                            Nombre NVARCHAR(100) NOT NULL,
                            Descripcion NVARCHAR(500),
                            FechaCreacion DATETIME DEFAULT GETDATE(),
                            Activo BIT DEFAULT 1
                        )",
                        
                        @"IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Productos]') AND type in (N'U'))
                        CREATE TABLE Productos (
                            Id INT IDENTITY(1,1) PRIMARY KEY,
                            CodigoBarras NVARCHAR(50) UNIQUE,
                            Nombre NVARCHAR(200) NOT NULL,
                            Descripcion NVARCHAR(500),
                            CategoriaId INT FOREIGN KEY REFERENCES Categorias(Id),
                            PrecioCompra DECIMAL(18,2) NOT NULL DEFAULT 0,
                            PrecioVenta DECIMAL(18,2) NOT NULL DEFAULT 0,
                            Stock INT NOT NULL DEFAULT 0,
                            StockMinimo INT NOT NULL DEFAULT 5,
                            FechaCreacion DATETIME DEFAULT GETDATE(),
                            Activo BIT DEFAULT 1
                        )",
                        
                        @"IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Usuarios]') AND type in (N'U'))
                        CREATE TABLE Usuarios (
                            Id INT IDENTITY(1,1) PRIMARY KEY,
                            Nombres NVARCHAR(100) NOT NULL,
                            Apellidos NVARCHAR(100),
                            Documento NVARCHAR(50),
                            TipoDocumento NVARCHAR(20),
                            Contraseña NVARCHAR(100) NOT NULL,
                            Rol NVARCHAR(50) DEFAULT 'Cajero',
                            Salario DECIMAL(18,2) NULL,
                            Activo BIT DEFAULT 1,
                            FechaCreacion DATETIME DEFAULT GETDATE()
                        )",
                        
                        @"IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Cajas]') AND type in (N'U'))
                        CREATE TABLE Cajas (
                            Id INT IDENTITY(1,1) PRIMARY KEY,
                            NumeroCaja INT NOT NULL DEFAULT 1,
                            Usuario NVARCHAR(100) NOT NULL,
                            MontoInicial DECIMAL(18,2) NOT NULL DEFAULT 0,
                            FechaApertura DATETIME NOT NULL DEFAULT GETDATE(),
                            FechaCierre DATETIME NULL,
                            MontoCierreEsperado DECIMAL(18,2) NULL,
                            MontoCierreReal DECIMAL(18,2) NULL,
                            Diferencia DECIMAL(18,2) NULL,
                            ObservacionesApertura NVARCHAR(500),
                            ObservacionesCierre NVARCHAR(500),
                            Estado NVARCHAR(20) NOT NULL DEFAULT 'Abierta'
                        )",
                        
                        @"IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Clientes]') AND type in (N'U'))
                        CREATE TABLE Clientes (
                            Id INT IDENTITY(1,1) PRIMARY KEY,
                            Documento NVARCHAR(50),
                            Nombre NVARCHAR(200) NOT NULL,
                            Telefono NVARCHAR(50),
                            Email NVARCHAR(100),
                            Direccion NVARCHAR(500),
                            FechaCreacion DATETIME DEFAULT GETDATE(),
                            Activo BIT DEFAULT 1
                        )",
                        
                        @"IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Ventas]') AND type in (N'U'))
                        CREATE TABLE Ventas (
                            Id INT IDENTITY(1,1) PRIMARY KEY,
                            NumeroVenta NVARCHAR(50) UNIQUE,
                            CajaId INT FOREIGN KEY REFERENCES Cajas(Id),
                            ClienteId INT FOREIGN KEY REFERENCES Clientes(Id),
                            FechaVenta DATETIME DEFAULT GETDATE(),
                            SubTotal DECIMAL(18,2) DEFAULT 0,
                            Impuesto DECIMAL(18,2) DEFAULT 0,
                            Total DECIMAL(18,2) DEFAULT 0,
                            MetodoPago NVARCHAR(50),
                            MontoPagado DECIMAL(18,2) DEFAULT 0,
                            Cambio DECIMAL(18,2) DEFAULT 0,
                            Usuario NVARCHAR(100),
                            Anulada BIT DEFAULT 0,
                            MotivoAnulacion NVARCHAR(500) NULL,
                            Origen NVARCHAR(20) DEFAULT 'Fisico',
                            Estado NVARCHAR(20) DEFAULT 'Confirmada',
                            Domicilio DECIMAL(18,2) DEFAULT 0,
                            SincronizadoAPI BIT DEFAULT 0
                        )",
                        
                        @"IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DetalleVentas]') AND type in (N'U'))
                        CREATE TABLE DetalleVentas (
                            Id INT IDENTITY(1,1) PRIMARY KEY,
                            VentaId INT FOREIGN KEY REFERENCES Ventas(Id),
                            ProductoId INT FOREIGN KEY REFERENCES Productos(Id),
                            Cantidad INT NOT NULL DEFAULT 1,
                            PrecioUnitario DECIMAL(18,2) NOT NULL DEFAULT 0,
                            CostoUnitario DECIMAL(18,2) NOT NULL DEFAULT 0,
                            SubTotal DECIMAL(18,2) NOT NULL DEFAULT 0,
                            Impuesto DECIMAL(18,2) NOT NULL DEFAULT 0,
                            Total DECIMAL(18,2) NOT NULL DEFAULT 0
                        )",
                        
                        @"IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MovimientosCaja]') AND type in (N'U'))
                        CREATE TABLE MovimientosCaja (
                            Id INT IDENTITY(1,1) PRIMARY KEY,
                            CajaId INT FOREIGN KEY REFERENCES Cajas(Id),
                            Tipo NVARCHAR(20) NOT NULL,
                            Concepto NVARCHAR(200) NOT NULL,
                            Monto DECIMAL(18,2) NOT NULL,
                            Fecha DATETIME DEFAULT GETDATE(),
                            Usuario NVARCHAR(100) NOT NULL
                        )",
                        
                        @"IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Stock]') AND type in (N'U'))
                        CREATE TABLE Stock (
                            Id INT IDENTITY(1,1) PRIMARY KEY,
                            ProductoId INT FOREIGN KEY REFERENCES Productos(Id),
                            Año INT NOT NULL,
                            Mes INT NOT NULL,
                            CantidadInicial INT DEFAULT 0,
                            CantidadEntrante INT DEFAULT 0,
                            CantidadSaliente INT DEFAULT 0,
                            CantidadFinal INT DEFAULT 0
                        )",
                        
                        @"IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ConteosFisicos]') AND type in (N'U'))
                        CREATE TABLE ConteosFisicos (
                            Id INT IDENTITY(1,1) PRIMARY KEY,
                            Fecha DATETIME DEFAULT GETDATE(),
                            Usuario NVARCHAR(100) NOT NULL,
                            Observaciones NVARCHAR(500)
                        )",
                        
                        @"IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DetalleConteoFisico]') AND type in (N'U'))
                        CREATE TABLE DetalleConteoFisico (
                            Id INT IDENTITY(1,1) PRIMARY KEY,
                            ConteoId INT FOREIGN KEY REFERENCES ConteosFisicos(Id),
                            ProductoId INT FOREIGN KEY REFERENCES Productos(Id),
                            StockSistema INT DEFAULT 0,
                            StockFisico INT DEFAULT 0
                        )",
                        
                        @"IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[InventarioMovimientos]') AND type in (N'U'))
                        CREATE TABLE InventarioMovimientos (
                            Id INT IDENTITY(1,1) PRIMARY KEY,
                            ProductoId INT FOREIGN KEY REFERENCES Productos(Id),
                            Tipo NVARCHAR(20) NOT NULL,
                            Cantidad INT NOT NULL,
                            StockAnterior INT NOT NULL,
                            StockNuevo INT NOT NULL,
                            Motivo NVARCHAR(200),
                            Fecha DATETIME DEFAULT GETDATE(),
                            Usuario NVARCHAR(100) NOT NULL
                        )",

                        @"DECLARE @constraintName NVARCHAR(200);
                        SELECT @constraintName = i.name FROM sys.indexes i
                        INNER JOIN sys.index_columns ic ON i.object_id = ic.object_id AND i.index_id = ic.index_id
                        INNER JOIN sys.columns c ON ic.object_id = c.object_id AND ic.column_id = c.column_id
                        WHERE i.object_id = OBJECT_ID('Productos') AND i.is_unique_constraint = 1 AND c.name = 'CodigoBarras';
                        IF @constraintName IS NOT NULL
                            EXEC('ALTER TABLE Productos DROP CONSTRAINT ' + @constraintName);",

                        @"IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ProductoCodigosBarras]') AND type in (N'U'))
                        CREATE TABLE ProductoCodigosBarras (
                            Id INT IDENTITY(1,1) PRIMARY KEY,
                            ProductoId INT FOREIGN KEY REFERENCES Productos(Id),
                            CodigoBarras NVARCHAR(50) NOT NULL
                        )",

                        @"IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Transacciones]') AND type in (N'U'))
                        CREATE TABLE Transacciones (
                            Id INT IDENTITY(1,1) PRIMARY KEY,
                            Fecha DATE NOT NULL DEFAULT GETDATE(),
                            Tipo NVARCHAR(20) NOT NULL,
                            Categoria NVARCHAR(100) NOT NULL,
                            Concepto NVARCHAR(500) NULL,
                            Monto DECIMAL(18,2) NOT NULL DEFAULT 0,
                            Usuario NVARCHAR(100) NULL,
                            FechaRegistro DATETIME DEFAULT GETDATE()
                        )",

                        @"IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Compras]') AND type in (N'U'))
                        CREATE TABLE Compras (
                            Id INT IDENTITY(1,1) PRIMARY KEY,
                            NumeroCompra NVARCHAR(50) UNIQUE,
                            FechaCompra DATETIME DEFAULT GETDATE(),
                            Proveedor NVARCHAR(200) NOT NULL,
                            SubTotal DECIMAL(18,2) DEFAULT 0,
                            Impuesto DECIMAL(18,2) DEFAULT 0,
                            Total DECIMAL(18,2) DEFAULT 0,
                            Usuario NVARCHAR(100)
                        )",

                        @"IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DetalleCompras]') AND type in (N'U'))
                        CREATE TABLE DetalleCompras (
                            Id INT IDENTITY(1,1) PRIMARY KEY,
                            CompraId INT FOREIGN KEY REFERENCES Compras(Id),
                            ProductoId INT FOREIGN KEY REFERENCES Productos(Id),
                            Cantidad INT NOT NULL DEFAULT 1,
                            PrecioUnitario DECIMAL(18,2) NOT NULL,
                            SubTotal DECIMAL(18,2) NOT NULL
                        )"
                    };

                    foreach (var sql in tablas)
                    {
                        try
                        {
                            connection.Execute(sql);
                        }
                        catch { }
                    }

                    // Crear ProductoVariantes si no existe
                    try
                    {
                        connection.Execute(@"IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ProductoVariantes]') AND type in (N'U'))
                        CREATE TABLE ProductoVariantes (
                            Id INT IDENTITY(1,1) PRIMARY KEY,
                            ProductoId INT NOT NULL FOREIGN KEY REFERENCES Productos(Id) ON DELETE CASCADE,
                            Nombre NVARCHAR(100) NOT NULL,
                            ColorHex NVARCHAR(7) NOT NULL DEFAULT '#000000',
                            Talla NVARCHAR(20) NULL,
                            Stock INT NULL,
                            ImagenUrl NVARCHAR(500) NULL,
                            Activo BIT NOT NULL DEFAULT 1,
                            Orden INT NOT NULL DEFAULT 0
                        )");
                    }
                    catch { }

                    var migraciones = new string[]
                    {
                        @"IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('ConteosFisicos') AND name = 'Estado')
                          ALTER TABLE ConteosFisicos ADD Estado NVARCHAR(20) DEFAULT 'Pendiente'",

                        @"IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('ConteosFisicos') AND name = 'ValorFaltante')
                          ALTER TABLE ConteosFisicos ADD ValorFaltante DECIMAL(18,2) DEFAULT 0",

                        @"IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('ConteosFisicos') AND name = 'ValorSobrante')
                          ALTER TABLE ConteosFisicos ADD ValorSobrante DECIMAL(18,2) DEFAULT 0",

                        @"IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('ConteosFisicos') AND name = 'TipoConteo')
                          ALTER TABLE ConteosFisicos ADD TipoConteo INT DEFAULT 1",

                        @"IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('ConteosFisicos') AND name = 'ConteoOriginalId')
                          ALTER TABLE ConteosFisicos ADD ConteoOriginalId INT NULL",

                        @"IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('DetalleConteoFisico') AND name = 'ValorFaltante')
                          ALTER TABLE DetalleConteoFisico ADD ValorFaltante DECIMAL(18,2) DEFAULT 0",

                        @"IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('DetalleConteoFisico') AND name = 'ValorSobrante')
                          ALTER TABLE DetalleConteoFisico ADD ValorSobrante DECIMAL(18,2) DEFAULT 0",

                        @"IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('DetalleConteoFisico') AND name = 'Diferencia')
                          ALTER TABLE DetalleConteoFisico ADD Diferencia AS (StockFisico - StockSistema)",

                        @"IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Usuarios') AND name = 'Salario')
                          ALTER TABLE Usuarios ADD Salario DECIMAL(18,2) NULL",

                        @"DELETE p1 FROM Productos p1
                          INNER JOIN Productos p2 ON p1.CodigoBarras = p2.CodigoBarras AND p1.CodigoBarras IS NOT NULL AND p1.CodigoBarras != ''
                          WHERE p1.Id > p2.Id",

                        @"IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Productos_CodigoBarras')
                          CREATE UNIQUE NONCLUSTERED INDEX IX_Productos_CodigoBarras ON Productos(CodigoBarras) WHERE CodigoBarras IS NOT NULL AND CodigoBarras != ''",

                        @"IF NOT EXISTS (SELECT * FROM Configuracion WHERE Clave = 'MARGEN_FIJOS')
                          INSERT INTO Configuracion (Clave, Valor, Descripcion) VALUES ('MARGEN_FIJOS', '25', 'Porcentaje para costos fijos (salarios, arriendo, servicios)')",

                        @"IF NOT EXISTS (SELECT * FROM Configuracion WHERE Clave = 'MARGEN_VARIABLES')
                          INSERT INTO Configuracion (Clave, Valor, Descripcion) VALUES ('MARGEN_VARIABLES', '5', 'Porcentaje para costos variables (bolsas, cajas, bisuteria)')",

                        @"IF NOT EXISTS (SELECT * FROM Configuracion WHERE Clave = 'MARGEN_UTILIDAD')
                          INSERT INTO Configuracion (Clave, Valor, Descripcion) VALUES ('MARGEN_UTILIDAD', '15', 'Porcentaje de utilidad o ganancia deseada')",

                        // Ventas: agregar columnas faltantes para sincronizacion
                        @"IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Ventas') AND name = 'Origen')
                          ALTER TABLE Ventas ADD Origen NVARCHAR(20) DEFAULT 'Fisico'",

                        @"IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Ventas') AND name = 'Estado')
                          ALTER TABLE Ventas ADD Estado NVARCHAR(20) DEFAULT 'Confirmada'",

                        @"IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Ventas') AND name = 'MotivoAnulacion')
                          ALTER TABLE Ventas ADD MotivoAnulacion NVARCHAR(500) NULL",

                        @"IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Ventas') AND name = 'Domicilio')
                          ALTER TABLE Ventas ADD Domicilio DECIMAL(18,2) DEFAULT 0",

                        // DetalleVentas: agregar columnas con nombres correctos
                        @"IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('DetalleVentas') AND name = 'PrecioUnitario')
                          ALTER TABLE DetalleVentas ADD PrecioUnitario DECIMAL(18,2) NOT NULL DEFAULT 0",

                        @"IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('DetalleVentas') AND name = 'CostoUnitario')
                          ALTER TABLE DetalleVentas ADD CostoUnitario DECIMAL(18,2) NOT NULL DEFAULT 0",

                        @"IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('DetalleVentas') AND name = 'SubTotal')
                          ALTER TABLE DetalleVentas ADD SubTotal DECIMAL(18,2) NOT NULL DEFAULT 0",

                        @"IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('DetalleVentas') AND name = 'Impuesto')
                          ALTER TABLE DetalleVentas ADD Impuesto DECIMAL(18,2) NOT NULL DEFAULT 0",

                        @"IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('DetalleVentas') AND name = 'Total')
                          ALTER TABLE DetalleVentas ADD Total DECIMAL(18,2) NOT NULL DEFAULT 0",

                        // Migrar datos viejos a columnas nuevas
                        @"UPDATE DetalleVentas SET PrecioUnitario = Precio WHERE PrecioUnitario = 0 AND Precio > 0",
                        @"UPDATE DetalleVentas SET SubTotal = Subtotal WHERE SubTotal = 0 AND Subtotal > 0",
                        @"UPDATE DetalleVentas SET Total = Subtotal WHERE Total = 0 AND Subtotal > 0",

                        // Tabla para cola de reintentos de sincronizacion
                        @"IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SincronizacionPendiente]') AND type in (N'U'))
                          CREATE TABLE SincronizacionPendiente (
                              Id INT IDENTITY(1,1) PRIMARY KEY,
                              VentaId INT NOT NULL,
                              JsonVenta NVARCHAR(MAX) NOT NULL,
                              JsonDetalles NVARCHAR(MAX) NOT NULL,
                              Intentos INT NOT NULL DEFAULT 0,
                              UltimoError NVARCHAR(MAX) NULL,
                              FechaCreacion DATETIME DEFAULT GETDATE(),
                              FechaUltimoIntento DATETIME NULL
                          )",

                        @"IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Ventas') AND name = 'SincronizadoAPI')
                          ALTER TABLE Ventas ADD SincronizadoAPI BIT DEFAULT 0",

                        // Productos: FechaModificacion para sincronizacion incremental
                        @"IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Productos') AND name = 'FechaModificacion')
                          ALTER TABLE Productos ADD FechaModificacion DATETIME DEFAULT GETDATE()",

                        // Variantes: agregar columna Talla para ropa interior
                        @"IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('ProductoVariantes') AND name = 'Talla')
                          ALTER TABLE ProductoVariantes ADD Talla NVARCHAR(20) NULL",

                        // DetalleVentas: agregar ProductoVarianteId
                        @"IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('DetalleVentas') AND name = 'ProductoVarianteId')
                          ALTER TABLE DetalleVentas ADD ProductoVarianteId INT NULL REFERENCES ProductoVariantes(Id)",
                    };

                    foreach (var sql in migraciones)
                    {
                        try
                        {
                            connection.Execute(sql);
                        }
                        catch { }
                    }
                }
            }
            catch { }
        }
    }
}
