-- =============================================
-- Script SQLite para Modulo de Contabilidad
-- =============================================

CREATE TABLE IF NOT EXISTS CuentasContables (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Codigo TEXT NOT NULL,
    Nombre TEXT NOT NULL,
    Tipo TEXT NOT NULL,
    Nivel INTEGER DEFAULT 1,
    CuentaPadreId INTEGER,
    Naturaleza TEXT DEFAULT 'Debe',
    Activo INTEGER DEFAULT 1,
    FechaCreacion DATETIME DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS AsientosContables (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    NumeroAsiento TEXT NOT NULL,
    Fecha DATE DEFAULT (date('now')),
    Concepto TEXT NOT NULL,
    Usuario TEXT NOT NULL,
    Origen TEXT DEFAULT 'Manual',
    OrigenId INTEGER,
    Estado TEXT DEFAULT 'Activo',
    FechaCreacion DATETIME DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS DetalleAsientoContable (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    AsientoId INTEGER NOT NULL,
    CuentaId INTEGER NOT NULL,
    Debe DECIMAL(18,2) DEFAULT 0,
    Haber DECIMAL(18,2) DEFAULT 0,
    Concepto TEXT,
    FOREIGN KEY (AsientoId) REFERENCES AsientosContables(Id),
    FOREIGN KEY (CuentaId) REFERENCES CuentasContables(Id)
);

PRINT 'Tablas contables SQLite creadas exitosamente!';
