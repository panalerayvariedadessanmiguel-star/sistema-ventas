# Version simplificada - usa POST individual /api/productos
Add-Type -AssemblyName System.Data

$apiUrl = "https://sistema-ventas-api-6x1w.onrender.com/api"
$sqlConn = "Server=(LocalDB)\MSSQLLocalDB;Database=SistemaVentasDB;Integrated Security=true;TrustServerCertificate=true"

try {
    $conn = New-Object System.Data.SqlClient.SqlConnection($sqlConn)
    $conn.Open()
    $cmd = $conn.CreateCommand()

    # Read categories
    Write-Host "Leyendo categorias..."
    $cmd.CommandText = "SELECT Nombre, Descripcion FROM Categorias WHERE Activo = 1"
    $reader = $cmd.ExecuteReader()
    $cats = @()
    while ($reader.Read()) {
        $n = $reader["Nombre"];$d = $reader["Descripcion"]
        if ($d -is [DBNull]){$d=""}
        $cats += @{nombre=$n.ToString();descripcion=$d.ToString()}
    }
    $reader.Close()

    # Post categories individually
    $catMap = @{}
    foreach ($c in $cats) {
        $body = @{nombre=$c.nombre;descripcion=$c.descripcion}
        try { $r = Invoke-RestMethod "$apiUrl/categorias" -Method Post -Body ($body|ConvertTo-Json) -ContentType "application/json" -TimeoutSec 10; $catMap[$c.nombre]=$r.id; Write-Host "  Cat OK: $($c.nombre)" -ForegroundColor Green }
        catch { try { $r = Invoke-RestMethod "$apiUrl/categorias" -TimeoutSec 10; $m = $r|Where-Object{$_.nombre -eq $c.nombre}; if($m){$catMap[$c.nombre]=$m.id; Write-Host "  Cat exists: $($c.nombre)" -ForegroundColor Gray}}catch{} }
    }

    # Read products - simple approach
    Write-Host "Leyendo productos..."
    $cmd.CommandText = "SELECT p.Id,p.CodigoBarras,p.Nombre,p.Descripcion,p.PrecioCompra,p.PrecioVenta,p.Stock,p.StockMinimo,c.Nombre AS NombreCategoria FROM Productos p LEFT JOIN Categorias c ON p.CategoriaId=c.Id WHERE p.Activo=1 ORDER BY p.Id"
    $reader = $cmd.ExecuteReader()
    $prods = @()
    while ($reader.Read()) {
        $prods += @{
            id=$reader["Id"]
            codigo=[string]::Join("",if($reader["CodigoBarras"]-is[DBNull]){""}else{$reader["CodigoBarras"]})
            nombre=[string]$reader["Nombre"]
            desc=[string]::Join("",if($reader["Descripcion"]-is[DBNull]){""}else{$reader["Descripcion"]})
            pCompra=[decimal]$reader["PrecioCompra"]
            pVenta=[decimal]$reader["PrecioVenta"]
            stock=[int]$reader["Stock"]
            stMin=if($reader["StockMinimo"]-is[DBNull]){5}else{[int]$reader["StockMinimo"]}
            catName=if($reader["NombreCategoria"]-is[DBNull]){"General"}else{[string]$reader["NombreCategoria"]}
        }
    }
    $reader.Close()
    $conn.Close()
    Write-Host "  $($prods.Count) productos"

    # Post products in batches via sync-pos
    $ok=0;$fail=0;$batch=@()
    foreach($p in $prods){
        $cid = $catMap[$p.catName]
        if(-not$cid){$cid=1}
        
        $nomEsc = $p.nombre -replace '"','\"' -replace "`n"," " -replace "`r"," "
        $descEsc = $p.desc -replace '"','\"' -replace "`n"," " -replace "`r"," "
        $codEsc = $p.codigo -replace '"','\"'

        $item = @{
            LocalId = $p.id
            CodigoBarras = $codEsc
            Nombre = $nomEsc
            Descripcion = $descEsc
            CategoriaId = $cid
            NombreCategoria = $p.catName
            PrecioCompra = [double]$p.pCompra
            PrecioVenta = [double]$p.pVenta
            Stock = [int]$p.stock
            StockMinimo = [int]$p.stMin
            ImagenUrl = ""
            Orden = 0
            Activo = $true
            FechaModificacion = (Get-Date -Format "yyyy-MM-ddTHH:mm:ssZ")
            Variantes = @()
        }
        $batch += $item

        if($batch.Count -ge 20){
            try{
                $json = $batch | ConvertTo-Json -Depth 5 -Compress
                $r = Invoke-RestMethod "$apiUrl/productos/sync-pos" -Method Post -Body $json -ContentType "application/json" -TimeoutSec 30
                $ok += $r.Count
                Write-Host "  OK batch: $($r.Count) productos" -ForegroundColor Green
                $batch = @()
            }catch{
                $fail += $batch.Count
                $msg = "  FAIL batch: $($_.Exception.Message)"
                Write-Host $msg -ForegroundColor Red
                $batch = @()
            }
        }
    }
    # Last batch
    if($batch.Count -gt 0){
        try{
            $json = $batch | ConvertTo-Json -Depth 5 -Compress
            $r = Invoke-RestMethod "$apiUrl/productos/sync-pos" -Method Post -Body $json -ContentType "application/json" -TimeoutSec 30
            $ok += $r.Count
            Write-Host "  OK batch final: $($r.Count) productos" -ForegroundColor Green
        }catch{
            $fail += $batch.Count
            $msg = "  FAIL batch final: $($_.Exception.Message)"
            Write-Host $msg -ForegroundColor Red
        }
    }

    Write-Host "`nFinalizado: $ok subidos, $fail errores" -ForegroundColor $(if($fail -eq 0){"Green"}else{"Yellow"})

} catch {
    Write-Host "ERROR: $($_.Exception.Message)" -ForegroundColor Red
}
Read-Host "Enter para salir"
