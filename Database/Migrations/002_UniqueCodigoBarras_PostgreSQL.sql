-- Eliminar duplicados: mantener el producto con el Id mas bajo por cada CodigoBarras no vacio
DELETE FROM Productos p1 USING Productos p2
WHERE p1.Id > p2.Id
  AND p1.CodigoBarras IS NOT NULL AND p1.CodigoBarras != ''
  AND p1.CodigoBarras = p2.CodigoBarras;

-- Crear indice unico parcial para evitar duplicados futuros
CREATE UNIQUE INDEX IF NOT EXISTS IX_Productos_CodigoBarras
ON Productos(CodigoBarras)
WHERE CodigoBarras IS NOT NULL AND CodigoBarras != '';
