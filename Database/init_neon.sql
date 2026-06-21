-- Tabla: Categorias
CREATE TABLE IF NOT EXISTS Categorias (
    Id SERIAL PRIMARY KEY, Nombre VARCHAR(100) NOT NULL,
    Descripcion VARCHAR(500), FechaCreacion TIMESTAMP DEFAULT NOW(),
    Activo BOOLEAN DEFAULT TRUE
);

-- Tabla: Productos
CREATE TABLE IF NOT EXISTS Productos (
    Id SERIAL PRIMARY KEY, CodigoBarras VARCHAR(50),
    Nombre VARCHAR(200) NOT NULL, Descripcion VARCHAR(500),
    CategoriaId INT REFERENCES Categorias(Id),
    PrecioCompra DECIMAL(18,2) NOT NULL DEFAULT 0,
    PrecioVenta DECIMAL(18,2) NOT NULL DEFAULT 0,
    Stock INT NOT NULL DEFAULT 0, StockMinimo INT NOT NULL DEFAULT 5,
    FechaCreacion TIMESTAMP DEFAULT NOW(),
    FechaModificacion TIMESTAMP DEFAULT NOW(),
    Activo BOOLEAN DEFAULT TRUE, ImagenUrl TEXT NOT NULL DEFAULT '',
    Orden INT NOT NULL DEFAULT 0
);
CREATE UNIQUE INDEX IF NOT EXISTS IX_Productos_CodigoBarras ON Productos(CodigoBarras) WHERE CodigoBarras IS NOT NULL AND CodigoBarras != '';

CREATE TABLE IF NOT EXISTS ProductoCodigosBarras (
    Id SERIAL PRIMARY KEY, ProductoId INT REFERENCES Productos(Id), CodigoBarras VARCHAR(50)
);

CREATE TABLE IF NOT EXISTS ProductoVariantes (
    Id SERIAL PRIMARY KEY, ProductoId INT NOT NULL REFERENCES Productos(Id) ON DELETE CASCADE,
    Nombre VARCHAR(100) NOT NULL, ColorHex VARCHAR(7) NOT NULL DEFAULT '#000000',
    Talla VARCHAR(20), Stock INT, ImagenUrl VARCHAR(500),
    Activo BOOLEAN NOT NULL DEFAULT TRUE, Orden INT NOT NULL DEFAULT 0
);

CREATE TABLE IF NOT EXISTS Cajas (
    Id SERIAL PRIMARY KEY, NumeroCaja INT NOT NULL DEFAULT 1,
    Usuario VARCHAR(100) NOT NULL, MontoInicial DECIMAL(18,2) NOT NULL DEFAULT 0,
    FechaApertura TIMESTAMP NOT NULL DEFAULT NOW(), FechaCierre TIMESTAMP,
    MontoCierreEsperado DECIMAL(18,2), MontoCierreReal DECIMAL(18,2),
    Diferencia DECIMAL(18,2), ObservacionesApertura VARCHAR(500),
    ObservacionesCierre VARCHAR(500), Estado VARCHAR(20) NOT NULL DEFAULT 'Abierta'
);

CREATE TABLE IF NOT EXISTS MovimientosCaja (
    Id SERIAL PRIMARY KEY, CajaId INT REFERENCES Cajas(Id),
    Tipo VARCHAR(20) NOT NULL, Concepto VARCHAR(200) NOT NULL,
    Monto DECIMAL(18,2) NOT NULL, Fecha TIMESTAMP DEFAULT NOW(),
    Usuario VARCHAR(100) NOT NULL
);

CREATE TABLE IF NOT EXISTS Clientes (
    Id SERIAL PRIMARY KEY, Documento VARCHAR(50) UNIQUE,
    Nombre VARCHAR(200) NOT NULL, Telefono VARCHAR(50), Email VARCHAR(200),
    Direccion VARCHAR(500), FechaRegistro TIMESTAMP DEFAULT NOW(),
    Activo BOOLEAN DEFAULT TRUE, Contrasena VARCHAR(200) NOT NULL DEFAULT ''
);

CREATE TABLE IF NOT EXISTS Ventas (
    Id SERIAL PRIMARY KEY, NumeroVenta VARCHAR(50) NOT NULL UNIQUE,
    CajaId INT DEFAULT 1, ClienteId INT REFERENCES Clientes(Id),
    FechaVenta TIMESTAMP NOT NULL DEFAULT NOW(),
    SubTotal DECIMAL(18,2) NOT NULL, Impuesto DECIMAL(18,2) NOT NULL DEFAULT 0,
    Total DECIMAL(18,2) NOT NULL, MetodoPago VARCHAR(50) NOT NULL,
    MontoPagado DECIMAL(18,2) NOT NULL, Cambio DECIMAL(18,2) NOT NULL DEFAULT 0,
    Usuario VARCHAR(100) NOT NULL, Anulada BOOLEAN DEFAULT FALSE,
    MotivoAnulacion VARCHAR(500), Origen VARCHAR(20) DEFAULT 'Web',
    Estado VARCHAR(20) DEFAULT 'Confirmada',
    Domicilio DECIMAL(18,2) NOT NULL DEFAULT 0,
    SincronizadoAPI BOOLEAN NOT NULL DEFAULT FALSE
);

CREATE TABLE IF NOT EXISTS DetalleVentas (
    Id SERIAL PRIMARY KEY, VentaId INT REFERENCES Ventas(Id),
    ProductoId INT REFERENCES Productos(Id), ProductoVarianteId INT,
    Cantidad INT NOT NULL, PrecioUnitario DECIMAL(18,2) NOT NULL,
    CostoUnitario DECIMAL(18,2) NOT NULL,
    SubTotal DECIMAL(18,2) NOT NULL, Impuesto DECIMAL(18,2) NOT NULL DEFAULT 0,
    Total DECIMAL(18,2) NOT NULL
);

CREATE TABLE IF NOT EXISTS POSSync (
    Id SERIAL PRIMARY KEY, VentaId INT NOT NULL REFERENCES Ventas(Id),
    ProductoId INT NOT NULL REFERENCES Productos(Id), ProductoVarianteId INT,
    Cantidad INT NOT NULL, Fecha TIMESTAMP NOT NULL DEFAULT NOW(),
    Sincronizado BOOLEAN NOT NULL DEFAULT FALSE
);

CREATE TABLE IF NOT EXISTS InventarioMovimientos (
    Id SERIAL PRIMARY KEY, ProductoId INT REFERENCES Productos(Id),
    Tipo VARCHAR(50) NOT NULL, Cantidad INT NOT NULL,
    StockAnterior INT NOT NULL, StockNuevo INT NOT NULL,
    Motivo VARCHAR(200), Fecha TIMESTAMP DEFAULT NOW(),
    Usuario VARCHAR(100) NOT NULL
);

CREATE TABLE IF NOT EXISTS Stock (
    Id SERIAL PRIMARY KEY, ProductoId INT REFERENCES Productos(Id),
    Anio INT NOT NULL, Mes INT NOT NULL,
    CantidadInicial INT NOT NULL DEFAULT 0, CantidadEntrante INT NOT NULL DEFAULT 0,
    CantidadSaliente INT NOT NULL DEFAULT 0, CantidadFinal INT NOT NULL DEFAULT 0,
    FechaRegistro TIMESTAMP DEFAULT NOW(), UNIQUE (ProductoId, Anio, Mes)
);

CREATE TABLE IF NOT EXISTS ConteosFisicos (
    Id SERIAL PRIMARY KEY, Fecha TIMESTAMP DEFAULT NOW(),
    Usuario VARCHAR(100) NOT NULL, Observaciones VARCHAR(500),
    Estado VARCHAR(20) DEFAULT 'Pendiente',
    ValorFaltante DECIMAL(18,2) DEFAULT 0, ValorSobrante DECIMAL(18,2) DEFAULT 0
);

CREATE TABLE IF NOT EXISTS DetalleConteoFisico (
    Id SERIAL PRIMARY KEY, ConteoId INT REFERENCES ConteosFisicos(Id),
    ProductoId INT REFERENCES Productos(Id), StockSistema INT NOT NULL,
    StockFisico INT NOT NULL,
    Diferencia INTEGER GENERATED ALWAYS AS (StockFisico - StockSistema) STORED,
    ValorFaltante DECIMAL(18,2) DEFAULT 0, ValorSobrante DECIMAL(18,2) DEFAULT 0,
    FechaRegistro TIMESTAMP DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS Configuracion (
    Id SERIAL PRIMARY KEY, Clave VARCHAR(100) NOT NULL UNIQUE,
    Valor VARCHAR(500), Descripcion VARCHAR(500)
);

CREATE TABLE IF NOT EXISTS Transaccion (
    Id SERIAL PRIMARY KEY, Fecha TIMESTAMP DEFAULT NOW(),
    Tipo VARCHAR(50) NOT NULL, Categoria VARCHAR(100),
    Concepto VARCHAR(200), Monto DECIMAL(18,2) NOT NULL, Usuario VARCHAR(100)
);

CREATE TABLE IF NOT EXISTS Usuarios (
    Id SERIAL PRIMARY KEY, Nombres VARCHAR(100) NOT NULL,
    Apellidos VARCHAR(100) NOT NULL, Documento VARCHAR(50),
    TipoDocumento VARCHAR(20), Contrasena VARCHAR(200) NOT NULL,
    Rol VARCHAR(20) DEFAULT 'Cajero', Salario DECIMAL(18,2),
    Activo BOOLEAN DEFAULT TRUE, FechaCreacion TIMESTAMP DEFAULT NOW()
);

-- Datos iniciales
INSERT INTO Categorias (Nombre, Descripcion) VALUES
    ('General', 'Categoria general'), ('Bebidas', 'Bebidas alcoholicas y no alcoholicas'),
    ('Alimentos', 'Alimentos en general'), ('Limpieza', 'Productos de limpieza'),
    ('Higiene', 'Productos de higiene personal')
ON CONFLICT DO NOTHING;

INSERT INTO Productos (CodigoBarras, Nombre, Descripcion, CategoriaId, PrecioCompra, PrecioVenta, Stock, StockMinimo) VALUES
    ('75010001', 'Agua 500ml', 'Botella de agua 500ml', 2, 0.50, 1.00, 100, 20),
    ('75010002', 'Refresco 600ml', 'Refresco de cola 600ml', 2, 0.80, 1.50, 80, 15),
    ('75010003', 'Pan Bimbo Grande', 'Pan de caja grande', 3, 2.50, 4.00, 30, 5),
    ('75010004', 'Arroz 1kg', 'Arroz blanco 1kg', 3, 1.00, 1.80, 50, 10),
    ('75010005', 'Detergente 1kg', 'Detergente en polvo 1kg', 4, 2.00, 3.50, 40, 8)
ON CONFLICT DO NOTHING;

INSERT INTO Configuracion (Clave, Valor, Descripcion) VALUES
    ('NIT', '123456789-0', 'Numero de Identificacion Tributaria'),
    ('DIRECCION', 'Calle 123 #45-67, Bogota D.C.', 'Direccion de la empresa'),
    ('TELEFONO', '601 123 4567', 'Telefono de contacto'),
    ('NOMBRE_EMPRESA', 'Sistema de Ventas SAS', 'Nombre de la empresa')
ON CONFLICT DO NOTHING;
