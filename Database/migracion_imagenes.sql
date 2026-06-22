-- Tabla para almacenar imágenes de productos persistentemente
CREATE TABLE IF NOT EXISTS Imagenes (
    Id SERIAL PRIMARY KEY,
    ProductoId INT REFERENCES Productos(Id) ON DELETE CASCADE,
    FileName VARCHAR(300) NOT NULL,
    Data BYTEA NOT NULL,
    MimeType VARCHAR(100) NOT NULL DEFAULT 'image/jpeg',
    FechaCreacion TIMESTAMP DEFAULT NOW(),
    UNIQUE(ProductoId, FileName)
);
