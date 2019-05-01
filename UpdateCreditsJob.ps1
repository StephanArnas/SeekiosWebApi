$SQLServer = "u2b7kcodrq.database.windows.net"
$SQLDBName = "seekios_db"
$uid ="weezo1505"
$pwd = "jR75=OBTaqp89"
$SqlQuery = "UPDATE dbo.seekiosProduction SET freeCredit = '60';"
$SqlConnection = New-Object System.Data.SqlClient.SqlConnection
$SqlConnection.ConnectionString = "Server = $SQLServer; Database = $SQLDBName; User ID = $uid; Password = $pwd;"
$SqlCmd = New-Object System.Data.SqlClient.SqlCommand
$SqlCmd.CommandText = $SqlQuery
$SqlCmd.Connection = $SqlConnection
$SqlAdapter = New-Object System.Data.SqlClient.SqlDataAdapter
$SqlAdapter.SelectCommand = $SqlCmd
$DataSet = New-Object System.Data.DataSet
$SqlAdapter.Fill($DataSet)