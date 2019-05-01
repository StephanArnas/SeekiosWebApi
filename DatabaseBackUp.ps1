$now = $(Get-Date).ToString("yyyyMMdd-HHmm")
$Logfile = "D:\home\site\Backup\logs\logs$now.log"

#source:http://fabriccontroller.net/backup-and-restore-your-sql-azure-database-using-powershell/

Function Write-Log
{
   Param ([string]$logstring)

   Add-content $Logfile -value $logstring
}

$SQLServer = "u2b7kcodrq.database.windows.net"
Write-Log "Sql server = $SqlServer"
$SQLDBName = "seekios_db"
Write-Log "DB Name = $SqlDBName"
$uid ="weezo1505"
Write-Log "uid = $uid"
$pwd = "jR75=OBTaqp89"
$ConnectionString = "Server = $SQLServer; Database = $SQLDBName; User ID = $uid; Password = $pwd;"

# Load DAC assembly.
$DacAssembly = "D:\home\site\Backup\dll\Microsoft.SqlServer.Dac.dll"
Write-Log "Loading Dac Assembly: $DacAssembly"
Try{  
		Add-Type -Path $DacAssembly 
	}
Catch{
		Write-Log "Cannot Add-Type for $DacAssembly"
		Write-Log $_.Exception.Message -ForegroundColor "red"
		Write-Log "Exiting"
		exit
	} 
Write-Log "Dac Assembly loaded."

# Initialize Dac service.
$Services = new-object Microsoft.SqlServer.Dac.DacServices $ConnectionString
if ($Services -eq $null)  
{
	Write-Log "Services could not be loaded"
	Write-Log "Exiting"
    exit
}

$Version = New-Object System.Version

# Start the actual export.
$start = $(Get-Date)
Write-Log "Starting backup at $SQLDBName at $start" -ForegroundColor "cyan"
$OutputFile = "D:\home\site\Backup\backups\seekios_db$now.bacpac"
Write-Log "Copying backup to : $OutputFile" -ForegroundColor "DarkGreen"
$Watch = New-Object System.Diagnostics.StopWatch
#$Watch.Start()
Try{
		Write-Log "Stated exporting database to $OutputFile"
		$Services.ExportBacpac($OutputFile, $SQLDBName)
	}
Catch{
		Write-Log "An error occured, cannot backup database" -ForegroundColor "red"
		Write-Log $_.Exception.Message -ForegroundColor "red"
		Write-Log "Exiting"
		exit
	}
Finally{
		#$Watch.Stop()
		$end = $(Get-Date) - $start
		Write-Log "Ended in : $end"
		$end = $end.ToString("HHh:MMm:SSs")
		Write-Log "Backup completed in $end" -ForegroundColor "green"
	}