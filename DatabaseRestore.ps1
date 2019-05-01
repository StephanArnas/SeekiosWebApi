param([string]$InputFile = $(throw "The InputFile parameter is required."))

$SqlInstallationFolder = "C:\Program Files (x86)\Microsoft SQL Server"
$SQLServer = "u1b7kcodrq.database.windows.net"
$SQLDBName = "seekios_db"
$uid ="weezo1505"
$pwd = "jR75=OBTaqp89"
$ConnectionString = "Server = $SQLServer; Database = $SQLDBName; User ID = $uid; Password = $pwd;"

# Load DAC assembly.
$DacAssembly = "C:\Program Files (x86)\Microsoft Visual Studio 14.0\Common7\IDE\Extensions\Microsoft\SQLDB\DAC\130\Microsoft.SqlServer.Dac.dll"
Write-Host "Loading Dac Assembly: $DacAssembly"  
Add-Type -Path $DacAssembly  
Write-Host "Dac Assembly loaded."

# Initialize Dac service.
$now = $(Get-Date).ToString("HH:mm:ss")
$Services = new-object Microsoft.SqlServer.Dac.DacServices $ConnectionString
if ($Services -eq $null)  
{
    exit
}


# Start the actual export.
Write-Host "Starting restore to $SQLDBName at $now"  
$Watch = New-Object System.Diagnostics.StopWatch
$Watch.Start()
$Package =  [Microsoft.SqlServer.Dac.BacPackage]::Load($InputFile)
$Services.ImportBacpac($Package, $SQLDBName)
$Package.Dispose()
$Watch.Stop()
Write-Host "Restore completed in" $Watch.Elapsed.ToString() 