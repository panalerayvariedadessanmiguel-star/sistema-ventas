-- Agrega columna Contrasena a la tabla Clientes
ALTER TABLE Clientes ADD COLUMN IF NOT EXISTS Contrasena VARCHAR(200) NOT NULL DEFAULT '';
