-- =============================================
-- Script de migracion para Conteo 2 (Reconteo)
-- Agrega soporte para segundo conteo fisico
-- =============================================

USE SistemaVentasDB;
GO

-- Agregar TipoConteo a ConteosFisicos (1 = Original, 2 = Reconteo)
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('ConteosFisicos') AND name = 'TipoConteo')
BEGIN
    ALTER TABLE ConteosFisicos ADD TipoConteo INT DEFAULT 1;
END
GO

-- Agregar ConteoOriginalId para vincular Reconteo con su Conteo original
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('ConteosFisicos') AND name = 'ConteoOriginalId')
BEGIN
    ALTER TABLE ConteosFisicos ADD ConteoOriginalId INT NULL;
END
GO

-- Crear FK si no existe
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('ConteosFisicos') AND name = 'ConteoOriginalId')
BEGIN
    IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_ConteosFisicos_Original')
    BEGIN
        ALTER TABLE ConteosFisicos ADD CONSTRAINT FK_ConteosFisicos_Original 
            FOREIGN KEY (ConteoOriginalId) REFERENCES ConteosFisicos(Id);
    END
END
GO

PRINT 'Migracion para Conteo 2 aplicada exitosamente!';
GO
