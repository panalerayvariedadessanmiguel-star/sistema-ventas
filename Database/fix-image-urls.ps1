Add-Type -AssemblyName System.Net.Http
$api = "https://sistema-ventas-api-6x1w.onrender.com/api"
$token = "MTpBZG1pbmlzdHJhZG9y"
$client = New-Object System.Net.Http.HttpClient
$client.DefaultRequestHeaders.Add("xAdminToken", $token)

$resp = $client.GetAsync("$api/productos").GetAwaiter().GetResult()
$products = ($resp.Content.ReadAsStringAsync().GetAwaiter().GetResult()) | ConvertFrom-Json

$fixed = 0
foreach ($p in $products) {
    if ($p.imagenUrl -and $p.imagenUrl.Contains("/api/api/")) {
        $newUrl = $p.imagenUrl.Replace("/api/api/", "/api/")
        $body = @{
            codigoBarras = $p.codigoBarras
            nombre = $p.nombre
            descripcion = $p.descripcion
            categoriaId = $p.categoriaId
            precioCompra = $p.precioCompra
            precioVenta = $p.precioVenta
            stock = $p.stock
            stockMinimo = $p.stockMinimo
            imagenUrl = $newUrl
            orden = $p.orden
            activo = $p.activo
        } | ConvertTo-Json
        $content = New-Object System.Net.Http.StringContent($body, [System.Text.Encoding]::UTF8, "application/json")
        $resp2 = $client.PutAsync("$api/productos/$($p.id)", $content).GetAwaiter().GetResult()
        if ($resp2.IsSuccessStatusCode) {
            Write-Host "OK: $($p.nombre) - URL corregida" -ForegroundColor Green
            $fixed++
        } else {
            Write-Host "FAIL: $($p.nombre) - $($resp2.StatusCode)" -ForegroundColor Red
        }
    }
}
Write-Host "`nCorregidas: $fixed URLs" -ForegroundColor Cyan
$client.Dispose()
