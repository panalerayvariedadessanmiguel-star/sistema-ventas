UPDATE Ventas SET Origen = 'Fisico' WHERE Origen = 'Web' OR Origen IS NULL;
SELECT @@ROWCOUNT AS Actualizados;
