-- Script SQLite para Sistema de Ventas
-- Tablas principales

CREATE TABLE IF NOT EXISTS Categorias (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Nombre TEXT NOT NULL,
    Descripcion TEXT,
    FechaCreacion DATETIME DEFAULT CURRENT_TIMESTAMP,
    Activo INTEGER DEFAULT 1
);

CREATE TABLE IF NOT EXISTS Productos (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    CodigoBarras TEXT,
    Nombre TEXT NOT NULL,
    Descripcion TEXT,
    CategoriaId INTEGER,
    PrecioCompra DECIMAL(18,2) NOT NULL DEFAULT 0,
    PrecioVenta DECIMAL(18,2) NOT NULL DEFAULT 0,
    Stock INTEGER NOT NULL DEFAULT 0,
    StockMinimo INTEGER NOT NULL DEFAULT 5,
    FechaCreacion DATETIME DEFAULT CURRENT_TIMESTAMP,
    Activo INTEGER DEFAULT 1,
    FOREIGN KEY (CategoriaId) REFERENCES Categorias(Id)
);

CREATE TABLE IF NOT EXISTS ProductoCodigosBarras (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    ProductoId INTEGER,
    CodigoBarras TEXT,
    FOREIGN KEY (ProductoId) REFERENCES Productos(Id)
);

CREATE TABLE IF NOT EXISTS ProductoVariantes (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    ProductoId INTEGER NOT NULL,
    Nombre TEXT NOT NULL,
    ColorHex TEXT NOT NULL DEFAULT '#000000',
    Stock INTEGER,
    ImagenUrl TEXT,
    Activo INTEGER NOT NULL DEFAULT 1,
    Orden INTEGER NOT NULL DEFAULT 0,
    FOREIGN KEY (ProductoId) REFERENCES Productos(Id) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS Cajas (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    NumeroCaja INTEGER NOT NULL DEFAULT 1,
    Usuario TEXT NOT NULL,
    MontoInicial DECIMAL(18,2) NOT NULL DEFAULT 0,
    FechaApertura DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FechaCierre DATETIME NULL,
    MontoCierreEsperado DECIMAL(18,2) NULL,
    MontoCierreReal DECIMAL(18,2) NULL,
    Diferencia DECIMAL(18,2) NULL,
    ObservacionesApertura TEXT,
    ObservacionesCierre TEXT,
    Estado TEXT NOT NULL DEFAULT 'Abierta'
);

CREATE TABLE IF NOT EXISTS MovimientosCaja (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    CajaId INTEGER,
    Tipo TEXT NOT NULL,
    Concepto TEXT NOT NULL,
    Monto DECIMAL(18,2) NOT NULL,
    Fecha DATETIME DEFAULT CURRENT_TIMESTAMP,
    Usuario TEXT NOT NULL,
    FOREIGN KEY (CajaId) REFERENCES Cajas(Id)
);

CREATE TABLE IF NOT EXISTS Clientes (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Documento TEXT UNIQUE,
    Nombre TEXT NOT NULL,
    Telefono TEXT,
    Email TEXT,
    Direccion TEXT,
    FechaRegistro DATETIME DEFAULT CURRENT_TIMESTAMP,
    Activo INTEGER DEFAULT 1
);

CREATE TABLE IF NOT EXISTS Ventas (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    NumeroVenta TEXT NOT NULL UNIQUE,
    CajaId INTEGER,
    ClienteId INTEGER,
    FechaVenta DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    SubTotal DECIMAL(18,2) NOT NULL,
    Impuesto DECIMAL(18,2) NOT NULL DEFAULT 0,
    Total DECIMAL(18,2) NOT NULL,
    MetodoPago TEXT NOT NULL,
    MontoPagado DECIMAL(18,2) NOT NULL,
    Cambio DECIMAL(18,2) NOT NULL DEFAULT 0,
    Usuario TEXT NOT NULL,
    Anulada INTEGER DEFAULT 0,
    MotivoAnulacion TEXT,
    FOREIGN KEY (CajaId) REFERENCES Cajas(Id),
    FOREIGN KEY (ClienteId) REFERENCES Clientes(Id)
);

CREATE TABLE IF NOT EXISTS DetalleVentas (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    VentaId INTEGER,
    ProductoId INTEGER,
    Cantidad INTEGER NOT NULL,
    PrecioUnitario DECIMAL(18,2) NOT NULL,
    CostoUnitario DECIMAL(18,2) NOT NULL,
    SubTotal DECIMAL(18,2) NOT NULL,
    Impuesto DECIMAL(18,2) NOT NULL DEFAULT 0,
    Total DECIMAL(18,2) NOT NULL,
    FOREIGN KEY (VentaId) REFERENCES Ventas(Id),
    FOREIGN KEY (ProductoId) REFERENCES Productos(Id)
);

CREATE TABLE IF NOT EXISTS InventarioMovimientos (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    ProductoId INTEGER,
    Tipo TEXT NOT NULL,
    Cantidad INTEGER NOT NULL,
    StockAnterior INTEGER NOT NULL,
    StockNuevo INTEGER NOT NULL,
    Motivo TEXT,
    Fecha DATETIME DEFAULT CURRENT_TIMESTAMP,
    Usuario TEXT NOT NULL,
    FOREIGN KEY (ProductoId) REFERENCES Productos(Id)
);

CREATE TABLE IF NOT EXISTS Stock (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    ProductoId INTEGER,
    Anio INTEGER NOT NULL,
    Mes INTEGER NOT NULL,
    CantidadInicial INTEGER NOT NULL DEFAULT 0,
    CantidadEntrante INTEGER NOT NULL DEFAULT 0,
    CantidadSaliente INTEGER NOT NULL DEFAULT 0,
    CantidadFinal INTEGER NOT NULL DEFAULT 0,
    FechaRegistro DATETIME DEFAULT CURRENT_TIMESTAMP,
    UNIQUE(ProductoId, Anio, Mes),
    FOREIGN KEY (ProductoId) REFERENCES Productos(Id)
);

CREATE TABLE IF NOT EXISTS ConteosFisicos (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Fecha DATETIME DEFAULT CURRENT_TIMESTAMP,
    Usuario TEXT NOT NULL,
    Observaciones TEXT,
    Estado TEXT DEFAULT 'Pendiente'
);

CREATE TABLE IF NOT EXISTS DetalleConteoFisico (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    ConteoId INTEGER,
    ProductoId INTEGER,
    StockSistema INTEGER NOT NULL,
    StockFisico INTEGER NOT NULL,
    FechaRegistro DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (ConteoId) REFERENCES ConteosFisicos(Id),
    FOREIGN KEY (ProductoId) REFERENCES Productos(Id)
);

CREATE TABLE IF NOT EXISTS Configuracion (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Clave TEXT NOT NULL UNIQUE,
    Valor TEXT,
    Descripcion TEXT
);

CREATE TABLE IF NOT EXISTS Usuarios (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Nombres TEXT NOT NULL,
    Apellidos TEXT NOT NULL,
    Documento TEXT NOT NULL UNIQUE,
    TipoDocumento TEXT NOT NULL,
    Contrasena TEXT NOT NULL,
    Rol TEXT NOT NULL,
    Salario REAL,
    Activo INTEGER DEFAULT 1,
    FechaCreacion DATETIME DEFAULT CURRENT_TIMESTAMP
);

-- Datos iniciales
INSERT OR IGNORE INTO Categorias (Id, Nombre, Descripcion) VALUES
(1, 'General', 'Categoria general'),
(2, 'Bebidas', 'Bebidas alcoholicas y no alcoholicas'),
(3, 'Alimentos', 'Alimentos en general'),
(4, 'Limpieza', 'Productos de limpieza'),
(5, 'Higiene', 'Productos de higiene personal');

INSERT OR IGNORE INTO Productos (Id, CodigoBarras, Nombre, Descripcion, CategoriaId, PrecioCompra, PrecioVenta, Stock, StockMinimo) VALUES
(1, '75010001', 'Agua 500ml', 'Botella de agua 500ml', 2, 0.50, 1.00, 100, 20),
(2, '75010002', 'Refresco 600ml', 'Refresco de cola 600ml', 2, 0.80, 1.50, 80, 15),
(3, '75010003', 'Pan Bimbo Grande', 'Pan de caja grande', 3, 2.50, 4.00, 30, 5),
(4, '75010004', 'Arroz 1kg', 'Arroz blanco 1kg', 3, 1.00, 1.80, 50, 10),
(5, '75010005', 'Detergente 1kg', 'Detergente en polvo 1kg', 4, 2.00, 3.50, 40, 8);

INSERT OR IGNORE INTO Configuracion (Clave, Valor, Descripcion) VALUES
('NIT', '123456789-0', 'Numero de Identificacion Tributaria'),
('DIRECCION', 'Calle 123 #45-67, Bogota D.C.', 'Direccion de la empresa'),
('TELEFONO', '601 123 4567', 'Telefono de contacto'),
('NOMBRE_EMPRESA', 'Sistema de Ventas SAS', 'Nombre de la empresa');

INSERT OR IGNORE INTO Usuarios (Documento, Nombres, Apellidos, TipoDocumento, Contrasena, Rol, Activo) VALUES
('12345678', 'Administrador', 'Sistema', 'CC', 'admin123', 'Administrador', 1);
