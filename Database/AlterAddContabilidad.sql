-- =============================================
-- Script de migracion para Modulo de Contabilidad
-- =============================================

USE SistemaVentasDB;
GO

-- Tabla: Plan de Cuentas
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CuentasContables]') AND type in (N'U'))
BEGIN
CREATE TABLE CuentasContables (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Codigo NVARCHAR(20) NOT NULL,
    Nombre NVARCHAR(200) NOT NULL,
    Tipo NVARCHAR(20) NOT NULL, -- Activo, Pasivo, Patrimonio, Ingreso, Gasto
    Nivel INT NOT NULL DEFAULT 1,
    CuentaPadreId INT NULL,
    Naturaleza NVARCHAR(10) NOT NULL DEFAULT 'Debe', -- Debe, Haber
    Activo BIT DEFAULT 1,
    FechaCreacion DATETIME DEFAULT GETDATE()
);
END
GO

-- Tabla: Asientos Contables (Cabecera)
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AsientosContables]') AND type in (N'U'))
BEGIN
CREATE TABLE AsientosContables (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    NumeroAsiento NVARCHAR(50) NOT NULL,
    Fecha DATE NOT NULL DEFAULT GETDATE(),
    Concepto NVARCHAR(500) NOT NULL,
    Usuario NVARCHAR(100) NOT NULL,
    Origen NVARCHAR(50) DEFAULT 'Manual',
    OrigenId INT NULL,
    Estado NVARCHAR(20) DEFAULT 'Activo',
    FechaCreacion DATETIME DEFAULT GETDATE()
);
END
GO

-- Tabla: Detalle Asientos Contables (Cuerpo)
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DetalleAsientoContable]') AND type in (N'U'))
BEGIN
CREATE TABLE DetalleAsientoContable (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    AsientoId INT NOT NULL FOREIGN KEY REFERENCES AsientosContables(Id),
    CuentaId INT NOT NULL FOREIGN KEY REFERENCES CuentasContables(Id),
    Debe DECIMAL(18,2) NOT NULL DEFAULT 0,
    Haber DECIMAL(18,2) NOT NULL DEFAULT 0,
    Concepto NVARCHAR(200) NULL
);
END
GO

PRINT 'Tablas contables creadas exitosamente!';
GO
