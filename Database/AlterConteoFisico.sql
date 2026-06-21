-- =============================================
-- Script de actualizacion para ConteosFisicos
-- Agregar campos ValorFaltante y ValorSobrante
-- =============================================

USE SistemaVentasDB;
GO

-- Agregar campos a ConteosFisicos
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('ConteosFisicos') AND name = 'ValorFaltante')
BEGIN
    ALTER TABLE ConteosFisicos ADD ValorFaltante DECIMAL(18,2) DEFAULT 0;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('ConteosFisicos') AND name = 'ValorSobrante')
BEGIN
    ALTER TABLE ConteosFisicos ADD ValorSobrante DECIMAL(18,2) DEFAULT 0;
END
GO

-- Agregar campos a DetalleConteoFisico
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('DetalleConteoFisico') AND name = 'ValorFaltante')
BEGIN
    ALTER TABLE DetalleConteoFisico ADD ValorFaltante DECIMAL(18,2) DEFAULT 0;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('DetalleConteoFisico') AND name = 'ValorSobrante')
BEGIN
    ALTER TABLE DetalleConteoFisico ADD ValorSobrante DECIMAL(18,2) DEFAULT 0;
END
GO

PRINT 'Campos ValorFaltante y ValorSobrante agregados exitosamente!';
GO
