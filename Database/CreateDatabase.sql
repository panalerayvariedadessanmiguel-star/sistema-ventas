-- =============================================
-- Script de Creacion de Base de Datos
-- Sistema de Ventas - SQL Server
-- =============================================

USE master;
GO

-- Crear base de datos
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'SistemaVentasDB')
BEGIN
    CREATE DATABASE SistemaVentasDB;
END
GO

USE SistemaVentasDB;
GO

-- =============================================
-- Tabla: Categorias
-- =============================================
CREATE TABLE Categorias (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Nombre NVARCHAR(100) NOT NULL,
    Descripcion NVARCHAR(500),
    FechaCreacion DATETIME DEFAULT GETDATE(),
    Activo BIT DEFAULT 1
);
GO

-- =============================================
-- Tabla: Productos
-- =============================================
CREATE TABLE Productos (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    CodigoBarras NVARCHAR(50),
    Nombre NVARCHAR(200) NOT NULL,
    Descripcion NVARCHAR(500),
    CategoriaId INT FOREIGN KEY REFERENCES Categorias(Id),
    PrecioCompra DECIMAL(18,2) NOT NULL DEFAULT 0,
    PrecioVenta DECIMAL(18,2) NOT NULL DEFAULT 0,
    Stock INT NOT NULL DEFAULT 0,
    StockMinimo INT NOT NULL DEFAULT 5,
    FechaCreacion DATETIME DEFAULT GETDATE(),
    Activo BIT DEFAULT 1
);
GO

GO

-- =============================================
-- Tabla: ProductoCodigosBarras (Multiples codigos por producto)
-- =============================================
CREATE TABLE ProductoCodigosBarras (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    ProductoId INT FOREIGN KEY REFERENCES Productos(Id),
    CodigoBarras NVARCHAR(50)
);
GO

-- =============================================
-- Tabla: ProductoVariantes (Colores/variantes para la web)
-- =============================================
CREATE TABLE ProductoVariantes (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    ProductoId INT NOT NULL FOREIGN KEY REFERENCES Productos(Id) ON DELETE CASCADE,
    Nombre NVARCHAR(100) NOT NULL,
    ColorHex NVARCHAR(7) NOT NULL DEFAULT '#000000',
    Talla NVARCHAR(20) NULL,
    Stock INT,
    ImagenUrl NVARCHAR(500),
    Activo BIT NOT NULL DEFAULT 1,
    Orden INT NOT NULL DEFAULT 0
);
GO

-- =============================================
-- Tabla: Cajas (Sesiones de caja)
-- =============================================
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
    Estado NVARCHAR(20) NOT NULL DEFAULT 'Abierta' -- Abierta, Cerrada
);
GO

-- =============================================
-- Tabla: MovimientosCaja (Entradas/Salidas de caja)
-- =============================================
CREATE TABLE MovimientosCaja (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    CajaId INT FOREIGN KEY REFERENCES Cajas(Id),
    Tipo NVARCHAR(20) NOT NULL, -- Entrada, Salida
    Concepto NVARCHAR(200) NOT NULL,
    Monto DECIMAL(18,2) NOT NULL,
    Fecha DATETIME DEFAULT GETDATE(),
    Usuario NVARCHAR(100) NOT NULL
);
GO

-- =============================================
-- Tabla: Clientes
-- =============================================
CREATE TABLE Clientes (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Documento NVARCHAR(50) UNIQUE,
    Nombre NVARCHAR(200) NOT NULL,
    Telefono NVARCHAR(50),
    Email NVARCHAR(200),
    Direccion NVARCHAR(500),
    FechaRegistro DATETIME DEFAULT GETDATE(),
    Activo BIT DEFAULT 1
);
GO

-- =============================================
-- Tabla: Ventas (Cabecera)
-- =============================================
CREATE TABLE Ventas (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    NumeroVenta NVARCHAR(50) NOT NULL UNIQUE,
    CajaId INT FOREIGN KEY REFERENCES Cajas(Id),
    ClienteId INT FOREIGN KEY REFERENCES Clientes(Id),
    FechaVenta DATETIME NOT NULL DEFAULT GETDATE(),
    SubTotal DECIMAL(18,2) NOT NULL,
    Impuesto DECIMAL(18,2) NOT NULL DEFAULT 0,
    Total DECIMAL(18,2) NOT NULL,
    MetodoPago NVARCHAR(50) NOT NULL, -- Efectivo, Tarjeta, Transferencia
    MontoPagado DECIMAL(18,2) NOT NULL,
    Cambio DECIMAL(18,2) NOT NULL DEFAULT 0,
    Usuario NVARCHAR(100) NOT NULL,
    Anulada BIT DEFAULT 0,
    MotivoAnulacion NVARCHAR(500)
);
GO

-- =============================================
-- Tabla: DetalleVentas
-- =============================================
CREATE TABLE DetalleVentas (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    VentaId INT FOREIGN KEY REFERENCES Ventas(Id),
    ProductoId INT FOREIGN KEY REFERENCES Productos(Id),
    Cantidad INT NOT NULL,
    PrecioUnitario DECIMAL(18,2) NOT NULL,
    CostoUnitario DECIMAL(18,2) NOT NULL,
    SubTotal DECIMAL(18,2) NOT NULL,
    Impuesto DECIMAL(18,2) NOT NULL DEFAULT 0,
    Total DECIMAL(18,2) NOT NULL
);
GO

-- =============================================
-- Tabla: InventarioMovimientos (Historico de stock)
-- =============================================
CREATE TABLE InventarioMovimientos (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    ProductoId INT FOREIGN KEY REFERENCES Productos(Id),
    Tipo NVARCHAR(50) NOT NULL, -- Entrada, Salida, Ajuste
    Cantidad INT NOT NULL,
    StockAnterior INT NOT NULL,
    StockNuevo INT NOT NULL,
    Motivo NVARCHAR(200),
    Fecha DATETIME DEFAULT GETDATE(),
    Usuario NVARCHAR(100) NOT NULL
);
GO

-- =============================================
-- Tabla: Stock (Control de stock por año/mes)
-- =============================================
CREATE TABLE Stock (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    ProductoId INT FOREIGN KEY REFERENCES Productos(Id),
    Año INT NOT NULL,
    Mes INT NOT NULL,
    CantidadInicial INT NOT NULL DEFAULT 0,
    CantidadEntrante INT NOT NULL DEFAULT 0,
    CantidadSaliente INT NOT NULL DEFAULT 0,
    CantidadFinal INT NOT NULL DEFAULT 0,
    FechaRegistro DATETIME DEFAULT GETDATE(),
    CONSTRAINT UQ_Stock_Producto_Año_Mes UNIQUE (ProductoId, Año, Mes)
);
GO

-- =============================================
-- Tabla: ConteosFisicos (Encabezado de conteo)
-- =============================================
CREATE TABLE ConteosFisicos (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Fecha DATETIME DEFAULT GETDATE(),
    Usuario NVARCHAR(100) NOT NULL,
    Observaciones NVARCHAR(500),
    Estado NVARCHAR(20) DEFAULT 'Pendiente', -- Pendiente, Finalizado
    ValorFaltante DECIMAL(18,2) DEFAULT 0,
    ValorSobrante DECIMAL(18,2) DEFAULT 0
);
GO

-- =============================================
-- Tabla: DetalleConteoFisico (Detalle del conteo)
-- =============================================
CREATE TABLE DetalleConteoFisico (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    ConteoId INT FOREIGN KEY REFERENCES ConteosFisicos(Id),
    ProductoId INT FOREIGN KEY REFERENCES Productos(Id),
    StockSistema INT NOT NULL,
    StockFisico INT NOT NULL,
    Diferencia AS (StockFisico - StockSistema),
    ValorFaltante DECIMAL(18,2) DEFAULT 0,
    ValorSobrante DECIMAL(18,2) DEFAULT 0,
    FechaRegistro DATETIME DEFAULT GETDATE()
);
GO

-- =============================================
-- Procedimiento: Obtener reporte mensual de utilidades
-- =============================================
CREATE PROCEDURE sp_ReporteUtilidadesMensual
    @Anio INT,
    @Mes INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        p.Nombre AS Producto,
        SUM(dv.Cantidad) AS CantidadVendida,
        SUM(dv.SubTotal) AS TotalVentas,
        SUM(dv.CostoUnitario * dv.Cantidad) AS TotalCosto,
        SUM(dv.Total - (dv.CostoUnitario * dv.Cantidad)) AS Utilidad
    FROM DetalleVentas dv
    INNER JOIN Productos p ON dv.ProductoId = p.Id
    INNER JOIN Ventas v ON dv.VentaId = v.Id
    WHERE YEAR(v.FechaVenta) = @Anio 
        AND MONTH(v.FechaVenta) = @Mes
        AND v.Anulada = 0
    GROUP BY p.Nombre
    ORDER BY Utilidad DESC;
END
GO

-- =============================================
-- Procedimiento: Obtener resumen de caja
-- =============================================
CREATE PROCEDURE sp_ResumenCaja
    @CajaId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        c.Id AS CajaId,
        c.Usuario,
        c.MontoInicial,
        c.FechaApertura,
        ISNULL(SUM(CASE WHEN mc.Tipo = 'Entrada' THEN mc.Monto ELSE 0 END), 0) AS TotalEntradas,
        ISNULL(SUM(CASE WHEN mc.Tipo = 'Salida' THEN mc.Monto ELSE 0 END), 0) AS TotalSalidas,
        ISNULL(SUM(CASE WHEN v.Anulada = 0 THEN v.Total ELSE 0 END), 0) AS TotalVentas,
        c.MontoInicial + ISNULL(SUM(CASE WHEN mc.Tipo = 'Entrada' THEN mc.Monto ELSE 0 END), 0) + 
        ISNULL(SUM(CASE WHEN v.Anulada = 0 THEN v.Total ELSE 0 END), 0) - 
        ISNULL(SUM(CASE WHEN mc.Tipo = 'Salida' THEN mc.Monto ELSE 0 END), 0) AS MontoEsperado
    FROM Cajas c
    LEFT JOIN MovimientosCaja mc ON c.Id = mc.CajaId
    LEFT JOIN Ventas v ON c.Id = v.CajaId
    WHERE c.Id = @CajaId
    GROUP BY c.Id, c.Usuario, c.MontoInicial, c.FechaApertura;
END
GO

-- =============================================
-- Datos iniciales
-- =============================================
INSERT INTO Categorias (Nombre, Descripcion) VALUES
('General', 'Categoria general'),
('Bebidas', 'Bebidas alcoholicas y no alcoholicas'),
('Alimentos', 'Alimentos en general'),
('Limpieza', 'Productos de limpieza'),
('Higiene', 'Productos de higiene personal');
GO

INSERT INTO Productos (CodigoBarras, Nombre, Descripcion, CategoriaId, PrecioCompra, PrecioVenta, Stock, StockMinimo) VALUES
('75010001', 'Agua 500ml', 'Botella de agua 500ml', 2, 0.50, 1.00, 100, 20),
('75010002', 'Refresco 600ml', 'Refresco de cola 600ml', 2, 0.80, 1.50, 80, 15),
('75010003', 'Pan Bimbo Grande', 'Pan de caja grande', 3, 2.50, 4.00, 30, 5),
('75010004', 'Arroz 1kg', 'Arroz blanco 1kg', 3, 1.00, 1.80, 50, 10),
('75010005', 'Detergente 1kg', 'Detergente en polvo 1kg', 4, 2.00, 3.50, 40, 8);
GO

-- =============================================
-- Tabla: Configuracion
-- =============================================
CREATE TABLE Configuracion (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Clave NVARCHAR(100) NOT NULL UNIQUE,
    Valor NVARCHAR(500),
    Descripcion NVARCHAR(500)
);
GO

-- Datos iniciales de configuracion
INSERT INTO Configuracion (Clave, Valor, Descripcion) VALUES
('NIT', '123456789-0', 'Numero de Identificacion Tributaria'),
('DIRECCION', 'Calle 123 #45-67, Bogota D.C.', 'Direccion de la empresa'),
('TELEFONO', '601 123 4567', 'Telefono de contacto'),
('NOMBRE_EMPRESA', 'Sistema de Ventas SAS', 'Nombre de la empresa');
GO

PRINT 'Base de datos SistemaVentasDB creada exitosamente!';
GO
