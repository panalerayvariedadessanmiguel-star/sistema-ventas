-- =============================================
-- Script de migracion SQLite para Conteo 2
-- Agrega soporte para segundo conteo fisico
-- =============================================

-- Agregar TipoConteo (1 = Original, 2 = Reconteo)
ALTER TABLE ConteosFisicos ADD COLUMN TipoConteo INTEGER DEFAULT 1;

-- Agregar ConteoOriginalId para vincular Reconteo con su Conteo original
ALTER TABLE ConteosFisicos ADD COLUMN ConteoOriginalId INTEGER REFERENCES ConteosFisicos(Id);

-- Agregar campos faltantes a ConteosFisicos (existen en SQL Server pero no en SQLite)
ALTER TABLE ConteosFisicos ADD COLUMN ValorFaltante DECIMAL(18,2) DEFAULT 0;
ALTER TABLE ConteosFisicos ADD COLUMN ValorSobrante DECIMAL(18,2) DEFAULT 0;

-- Agregar campos faltantes a DetalleConteoFisico
ALTER TABLE DetalleConteoFisico ADD COLUMN Diferencia INTEGER DEFAULT 0;
ALTER TABLE DetalleConteoFisico ADD COLUMN ValorFaltante DECIMAL(18,2) DEFAULT 0;
ALTER TABLE DetalleConteoFisico ADD COLUMN ValorSobrante DECIMAL(18,2) DEFAULT 0;

PRINT 'Migracion SQLite para Conteo 2 aplicada exitosamente!';
