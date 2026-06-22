Add-Type -Path "$env:USERPROFILE\.nuget\packages\npgsql\10.0.3\lib\net10.0\Npgsql.dll"
Add-Type -AssemblyName System.Data

$connStr = "Host=ep-ancient-credit-aicwplyb-pooler.c-4.us-east-1.aws.neon.tech;Database=neondb;Username=neondb_owner;Password=npg_TJ4QN1xmzeFp;SSL Mode=Require"
$conn = New-Object Npgsql.NpgsqlConnection($connStr)
$conn.Open()

$sql = @"
CREATE TABLE IF NOT EXISTS Imagenes (
    Id SERIAL PRIMARY KEY,
    ProductoId INT REFERENCES Productos(Id) ON DELETE CASCADE,
    FileName VARCHAR(300) NOT NULL,
    Data BYTEA NOT NULL,
    MimeType VARCHAR(100) NOT NULL DEFAULT 'image/jpeg',
    FechaCreacion TIMESTAMP DEFAULT NOW(),
    UNIQUE(ProductoId, FileName)
);
"@

$cmd = $conn.CreateCommand()
$cmd.CommandText = $sql
$cmd.ExecuteNonQuery()
Write-Host "Tabla Imagenes creada/verificada" -ForegroundColor Green
$conn.Close()
