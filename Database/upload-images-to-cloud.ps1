param(
    [Parameter(Mandatory = $true)]
    [string]$BlobToken,

    [Parameter(Mandatory = $false)]
    [string]$ApiUrl = "https://sistema-ventas-api-6x1w.onrender.com/api",

    [Parameter(Mandatory = $false)]
    [string]$ImagesDir = "C:\SistemaVentas\Imagenes"
)

Add-Type -AssemblyName System.Data
Add-Type -AssemblyName System.Net.Http

Write-Host "=== Subir imagenes a Vercel Blob ===" -ForegroundColor Cyan
Write-Host "ApiUrl: $ApiUrl"
Write-Host "ImagesDir: $ImagesDir"
Write-Host ""

# 1. Leer productos con imagen de SQL Server local
$conn = New-Object System.Data.SqlClient.SqlConnection("Server=(LocalDB)\MSSQLLocalDB;Database=SistemaVentasDB;Integrated Security=true;TrustServerCertificate=true")
$conn.Open()
$cmd = $conn.CreateCommand()
$cmd.CommandText = "SELECT p.Id, p.Nombre, p.ImagenUrl FROM Productos p WHERE p.ImagenUrl IS NOT NULL AND p.ImagenUrl != '' AND p.Activo = 1"
$reader = $cmd.ExecuteReader()
$products = @()
while ($reader.Read()) {
    $products += @{
        id = $reader["Id"]
        nombre = $reader["Nombre"]
        imagenUrl = $reader["ImagenUrl"]
    }
}
$reader.Close()
$conn.Close()
Write-Host "Productos con imagen en BD local: $($products.Count)" -ForegroundColor Yellow

# 2. Listar archivos de imagen disponibles
$imageFiles = @{}
Get-ChildItem -Path $ImagesDir -File | ForEach-Object {
    $imageFiles[$_.Name.ToLowerInvariant()] = $_.FullName
    $imageFiles[$_.BaseName.ToLowerInvariant()] = $_.FullName
}
Write-Host "Archivos de imagen en disco: $($imageFiles.Count / 2)" -ForegroundColor Yellow

# 3. Obtener productos de la nube (para mapear por nombre)
$client = New-Object System.Net.Http.HttpClient
try {
    $resp = $client.GetAsync("$ApiUrl/productos").GetAwaiter().GetResult()
    $json = $resp.Content.ReadAsStringAsync().GetAwaiter().GetResult()
    $cloudProducts = $json | ConvertFrom-Json
    Write-Host "Productos en nube: $($cloudProducts.Count)" -ForegroundColor Yellow

    # Crear mapa nombre -> producto cloud
    $cloudByName = @{}
    $cloudProducts | ForEach-Object { $cloudByName[$_.nombre] = $_ }

    # 4. Subir imagenes
    $uploaded = 0
    $skipped = 0
    $failed = 0

    foreach ($p in $products) {
        $nombre = $p.nombre
        $cloudProd = $cloudByName[$nombre]

        if (-not $cloudProd) {
            Write-Host "  SKIP: '$nombre' no encontrado en nube" -ForegroundColor DarkYellow
            $skipped++
            continue
        }

        # Determinar archivo de imagen
        $fileName = ""
        $localPath = ""

        # Estrategia 1: Extraer nombre de archivo de ImagenUrl
        if ($p.imagenUrl -match "/([^/]+\.\w+)$") {
            $fileName = $matches[1]
            $fileNameDecoded = [System.Uri]::UnescapeDataString($fileName)
            $localPath = Join-Path $ImagesDir $fileNameDecoded
            if (-not (Test-Path $localPath)) {
                $localPath = Join-Path $ImagesDir $fileName
            }
        }

        # Estrategia 2: Buscar por nombre del producto
        if (-not (Test-Path $localPath)) {
            foreach ($key in $imageFiles.Keys) {
                if ($key -eq $nombre.ToLowerInvariant() -or $key.StartsWith($nombre.ToLowerInvariant())) {
                    $localPath = $imageFiles[$key]
                    break
                }
            }
        }

        if (-not (Test-Path $localPath)) {
            Write-Host "  SKIP: '$nombre' - archivo no encontrado" -ForegroundColor DarkYellow
            $skipped++
            continue
        }

        # Subir a Vercel Blob via REST API
        $ext = [System.IO.Path]::GetExtension($localPath)
        $blobName = [System.Uri]::EscapeDataString($nombre) + $ext
        $blobUrl = "https://blob.vercel-storage.com/$blobName"

        try {
            $bytes = [System.IO.File]::ReadAllBytes($localPath)
            $content = New-Object System.Net.Http.ByteArrayContent($bytes)
            $content.Headers.ContentType = [System.Net.Http.Headers.MediaTypeHeaderValue]::Parse("image/jpeg")
            $request = New-Object System.Net.Http.HttpRequestMessage
            $request.Method = [System.Net.Http.HttpMethod]::Put
            $request.RequestUri = $blobUrl
            $request.Headers.Authorization = New-Object System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", $BlobToken)
            $request.Content = $content

            $resp2 = $client.SendAsync($request).GetAwaiter().GetResult()
            $resp2.EnsureSuccessStatusCode()

            $publicUrl = $resp2.Headers.Location -or ($blobUrl -replace "^https://blob\.vercel-storage\.com/", "https://${blobName}.public.blob.vercel-storage.com/")
            # Try to get the final URL from response
            $finalUrl = $resp2.RequestMessage.RequestUri.ToString()
            if ($resp2.Headers.Location) {
                $finalUrl = $resp2.Headers.Location.ToString()
            }

            Write-Host "  OK: '$nombre' -> $finalUrl" -ForegroundColor Green

            # 5. Actualizar cloud DB
            $updateBody = $cloudProd | ConvertTo-Json -Depth 3
            $updateObj = $updateBody | ConvertFrom-Json
            $updateObj.imagenUrl = $finalUrl
            $updateJson = $updateObj | ConvertTo-Json -Depth 3

            # Need xAdminToken - try to get from env or skip
            # For now just record the URL
            $uploaded++
        }
        catch {
            Write-Host "  FAIL: '$nombre' - $_" -ForegroundColor Red
            $failed++
        }
    }

    Write-Host ""
    Write-Host "=== Resumen ===" -ForegroundColor Cyan
    Write-Host "Subidas: $uploaded" -ForegroundColor Green
    Write-Host "Saltadas: $skipped" -ForegroundColor Yellow
    Write-Host "Fallidas: $failed" -ForegroundColor $(if($failed -gt 0){"Red"}else{"Green"})

    if ($uploaded -gt 0) {
        Write-Host ""
        Write-Host "IMPORTANTE: Las URLs de imagen se subieron a Vercel Blob pero falta" -ForegroundColor Yellow
        Write-Host "actualizar la BD cloud. Ejecuta el siguiente paso cuando tengas el" -ForegroundColor Yellow
        Write-Host "xAdminToken (revisa el POS local para el token de administrador)." -ForegroundColor Yellow
    }
}
finally {
    $client.Dispose()
}
