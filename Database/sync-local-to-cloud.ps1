# Script para migrar productos locales (SQL Server) a la nube (Neon PostgreSQL)
# Ejecutar UNA SOLA VEZ en la maquina local

Add-Type -AssemblyName System.Data

$apiUrl = "https://sistema-ventas-api-6x1w.onrender.com/api"
$sqlConn = "Server=(LocalDB)\MSSQLLocalDB;Database=SistemaVentasDB;Integrated Security=true;TrustServerCertificate=true"

try {
    Write-Host "Conectando a SQL Server local..." -ForegroundColor Cyan
    $conn = New-Object System.Data.SqlClient.SqlConnection($sqlConn)
    $conn.Open()

    # 1. Subir categorias
    Write-Host "`nSubiendo categorias..." -ForegroundColor Yellow
    $cmd = $conn.CreateCommand()
    $cmd.CommandText = "SELECT Id, Nombre, Descripcion FROM Categorias WHERE Activo = 1 ORDER BY Id"
    $reader = $cmd.ExecuteReader()
    $categorias = @()
    while ($reader.Read()) {
        $categorias += @{
            id = 0
            nombre = $reader["Nombre"].ToString()
            descripcion = if ($reader["Descripcion"] -ne [DBNull]) { $reader["Descripcion"].ToString() } else { "" }
        }
    }
    $reader.Close()
    Write-Host "  Encontradas $($categorias.Count) categorias"

    # Mapa de IDs locales a IDs remotos
    $catMap = @{}
    foreach ($cat in $categorias) {
        try {
            $body = @{ nombre = $cat.nombre; descripcion = $cat.descripcion } | ConvertTo-Json
            $resp = Invoke-WebRequest -Uri "$apiUrl/categorias" -Method Post -Body $body -ContentType "application/json" -TimeoutSec 10
            $result = $resp.Content | ConvertFrom-Json
            if ($result.id) {
                $catMap[$result.nombre] = $result.id
                Write-Host "  OK: $($cat.nombre) -> ID remoto $($result.id)" -ForegroundColor Green
            }
        } catch {
            # Puede que ya exista, intentar obtener el ID
            try {
                $resp2 = Invoke-WebRequest -Uri "$apiUrl/categorias" -TimeoutSec 10
                $cats = $resp2.Content | ConvertFrom-Json
                $match = $cats | Where-Object { $_.nombre -eq $cat.nombre }
                if ($match) {
                    $catMap[$cat.nombre] = $match.id
                    Write-Host "  Ya existe: $($cat.nombre) -> ID $($match.id)" -ForegroundColor Gray
                }
            } catch {
                Write-Host "  Error con categoria $($cat.nombre): $($_.Exception.Message)" -ForegroundColor Red
            }
        }
    }

    # 2. Obtener productos con categorias
    Write-Host "`nObteniendo productos de SQL Server..." -ForegroundColor Yellow
    $cmd.CommandText = @"
        SELECT p.Id, p.CodigoBarras, p.Nombre, p.Descripcion, p.CategoriaId,
               p.PrecioCompra, p.PrecioVenta, p.Stock, p.StockMinimo,
               p.Activo, c.Nombre AS NombreCategoria
        FROM Productos p
        LEFT JOIN Categorias c ON p.CategoriaId = c.Id
        ORDER BY p.Id
"@
    $reader = $cmd.ExecuteReader()
    $productos = @()
    while ($reader.Read()) {
        $productos += @{
            LocalId = [int]$reader["Id"]
            CodigoBarras = if ($reader["CodigoBarras"] -ne [DBNull]) { $reader["CodigoBarras"].ToString() } else { "" }
            Nombre = $reader["Nombre"].ToString()
            Descripcion = if ($reader["Descripcion"] -ne [DBNull]) { $reader["Descripcion"].ToString() } else { "" }
            CategoriaId = if ($reader["CategoriaId"] -ne [DBNull]) { [int]$reader["CategoriaId"] } else { 1 }
            NombreCategoria = if ($reader["NombreCategoria"] -ne [DBNull]) { $reader["NombreCategoria"].ToString() } else { "General" }
            PrecioCompra = if ($reader["PrecioCompra"] -ne [DBNull]) { [decimal]$reader["PrecioCompra"] } else { 0 }
            PrecioVenta = if ($reader["PrecioVenta"] -ne [DBNull]) { [decimal]$reader["PrecioVenta"] } else { 0 }
            Stock = if ($reader["Stock"] -ne [DBNull]) { [int]$reader["Stock"] } else { 0 }
            StockMinimo = if ($reader["StockMinimo"] -ne [DBNull]) { [int]$reader["StockMinimo"] } else { 5 }
            Activo = if ($reader["Activo"] -ne [DBNull]) { [bool]$reader["Activo"] } else { $true }
            Variantes = @()
        }
    }
    $reader.Close()
    Write-Host "  Encontrados $($productos.Count) productos"

    # 3. Obtener variantes
    Write-Host "`nObteniendo variantes..." -ForegroundColor Yellow
    $cmd.CommandText = "SELECT Id, ProductoId, Nombre, ColorHex, Talla, Stock, Activo, Orden FROM ProductoVariantes ORDER BY ProductoId, Orden"
    $reader = $cmd.ExecuteReader()
    $variantes = @{}
    while ($reader.Read()) {
        $prodId = [int]$reader["ProductoId"]
        if (-not $variantes.ContainsKey($prodId)) { $variantes[$prodId] = @() }
    $variantes[$prodId] += @{
        Id = [int]$reader["Id"]
        ProductoId = $prodId
        Nombre = $reader["Nombre"].ToString()
        ColorHex = if ($reader["ColorHex"] -ne [DBNull]) { $reader["ColorHex"].ToString() } else { "#000000" }
        Talla = if ($reader["Talla"] -ne [DBNull]) { $reader["Talla"].ToString() } else { "" }
        Stock = if ($reader["Stock"] -ne [DBNull]) { [int]$reader["Stock"] } else { $null }
        ImagenUrl = ""
        Activo = if ($reader["Activo"] -ne [DBNull]) { [bool]$reader["Activo"] } else { $true }
        Orden = if ($reader["Orden"] -ne [DBNull]) { [int]$reader["Orden"] } else { 0 }
    }
    }
    $reader.Close()
    $totalVariantes = ($variantes.Values | ForEach-Object { $_.Count }) | Measure-Object -Sum | Select-Object -ExpandProperty Sum
    Write-Host "  Encontradas $totalVariantes variantes"

    $conn.Close()

    # 4. Subir productos en lote
    Write-Host "`nSubiendo productos a la nube..." -ForegroundColor Yellow
    $batchSize = 50
    $total = $productos.Count
    $synced = 0
    $errors = 0

    for ($i = 0; $i -lt $total; $i += $batchSize) {
        $batch = $productos[$i..([Math]::Min($i + $batchSize - 1, $total - 1))]
        $payload = @()

        foreach ($p in $batch) {
            $pVariantes = if ($variantes.ContainsKey($p.LocalId)) { $variantes[$p.LocalId] } else { @() }
            $payload += @{
                LocalId = $p.LocalId
                CodigoBarras = $p.CodigoBarras
                Nombre = $p.Nombre
                Descripcion = $p.Descripcion
                CategoriaId = $catMap[$p.NombreCategoria]
                NombreCategoria = $p.NombreCategoria
                PrecioCompra = $p.PrecioCompra
                PrecioVenta = $p.PrecioVenta
                Stock = $p.Stock
                StockMinimo = $p.StockMinimo
                ImagenUrl = ""
                Orden = 0
                Activo = $p.Activo
                FechaModificacion = (Get-Date).ToString("o")
                Variantes = $pVariantes
            }
        }

        try {
            $body = $payload | ConvertTo-Json -Depth 5
            $resp = Invoke-WebRequest -Uri "$apiUrl/productos/sync-pos" -Method Post -Body $body -ContentType "application/json" -TimeoutSec 30
            $result = $resp.Content | ConvertFrom-Json
            $synced += $result.Count
            Write-Host "  Lote $([Math]::Floor($i/$batchSize)+1): $($result.Count) productos sincronizados" -ForegroundColor Green
        } catch {
            $errors += $batch.Count
            Write-Host "  Error en lote $([Math]::Floor($i/$batchSize)+1): $($_.Exception.Message)" -ForegroundColor Red
        }
    }

    Write-Host "`n==============================" -ForegroundColor Cyan
    Write-Host "SINCRONIZACION COMPLETADA" -ForegroundColor Green
    Write-Host "  Productos subidos: $synced" -ForegroundColor Green
    if ($errors -gt 0) { Write-Host "  Errores: $errors" -ForegroundColor Red }
    Write-Host "==============================" -ForegroundColor Cyan

} catch {
    Write-Host "ERROR GENERAL: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Detalle: $($_.Exception.StackTrace)" -ForegroundColor DarkRed
}

Read-Host "`nPresiona Enter para salir"
