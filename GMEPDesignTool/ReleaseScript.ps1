# Requires https://dev.mysql.com/downloads/connector/net/
$type = Read-Host "Enter the release type (major, minor, patch)"

if ($type -ine "major") {
    if ($type -ine "minor") {
        if ($type -ine "patch") {
            Write-Host "Invalid release type. Only 'major', 'minor', 'patch' allowed."
            Read-Host "Press Enter to close"
            exit 1
        }
    }
}

# Update version
$pass = Get-Content -Path "Properties/Settings.Designer.cs" | Select-String -Pattern "(?<=Password=)[0-9A-Za-z]+" | ForEach-Object { $_.Matches.Value }
$server = "gmep-design-tool-test.ch8c88cauy2x.us-west-1.rds.amazonaws.com"
$user = "admin"
$db = "gmep-design-tool"

$cs = "server=$server;port=3306;user id=$user;password=$pass;database=$db;pooling=false"

[void][Reflection.Assembly]::LoadWithPartialName('MySQL.Data')

$cn = New-Object MySql.Data.MySqlClient.MySqlConnection
$cn.ConnectionString = $cs
$cn.Open()

$cmd= New-Object MySql.Data.MySqlClient.MySqlCommand
$cmd.Connection  = $cn
$cmd.CommandText = 'SELECT version_no FROM `gmep_design_tool_versions` ORDER BY date_created DESC LIMIT 1'
$reader = $cmd.ExecuteReader()
$currentVersion = ""
while ($reader.Read()) {
    $currentVersion = $reader["version_no"]
}

$reader.Close()
$cn.Close()

$versionArr = $currentVersion -split "\."

$currentMajorVersionStr = $versionArr[0]
$currentMajorVersion = [int]$currentMajorVersionStr
$currentMinorVersionStr = $versionArr[1]
$currentMinorVersion = [int]$currentMinorVersionStr
$currentPatchVersionStr = $versionArr[2]
$currentPatchVersion = [int]$currentPatchVersionStr
$nextVersion = ""
if ($type -ieq "major") {
    $nextMajorVersion = $currentMajorVersion + 1
    $nextMajorVersionStr = $nextMajorVersion.ToString()
    if ($nextMajorVersion -lt 10) {
        $nextMajorVersionStr = "0$nextMajorVersionStr"
    }
    $nextVersion = "$nextMajorVersionStr.00.00"
}
if ($type -ieq "minor") {
    $nextMinorVersion = $currentMinorVersion + 1
    $nextMinorVersionStr = $nextMinorVersion.ToString()
    if ($nextMinorVersion -lt 10) {
        $nextMinorVersionStr = "0$nextMinorVersionStr"
    }
    $nextVersion = "$currentMajorVersionStr.$nextMinorVersionStr.00"
}
if ($type -ieq "patch") {
    $nextPatchVersion = $currentPatchVersion + 1
    $nextPatchVersionStr = $nextPatchVersion.ToString()
    if ($nextPatchVersion -lt 10) {
        $nextPatchVersionStr = "0$nextPatchVersionStr"
    }
    $nextVersion = "$currentMajorVersionStr.$currentMinorVersionStr.$nextPatchVersionStr"
}

$cn = New-Object MySql.Data.MySqlClient.MySqlConnection
$cn.ConnectionString = $cs
$cn.Open()

$cmd= New-Object MySql.Data.MySqlClient.MySqlCommand
$cmd.Connection  = $cn
$cmd.CommandText = "INSERT INTO gmep_design_tool_versions (id, version_no) VALUES (null, @nextVersion)"
$cmd.Parameters.AddWithValue("@nextVersion", $nextVersion) 
$cmd.ExecuteNonQuery()

$cn.Close()


# Release
$zDriveDestDir = "Z:\GMEP Engineers\Users\GMEP Softwares\AutoCAD Commands\GMEPDesignTool"

$date = Get-Date -Format "yyyy-MM-dd HH-mm-ss"
mkdir -Force -Path "$zDriveDestDir\$date-$nextVersion"

Write-Output "$nextVersion" | Out-File -FilePath "bin\Debug\net8.0-windows\version.txt" -Force

Compress-Archive -Force -Path "bin", "Symbols", "Install.ps1", "gmep.ico" -DestinationPath "GMEPDesignTool.zip"
Copy-Item "GMEPDesignTool.zip" -Destination "Z:\GMEP Engineers\Users\GMEP Softwares"
Copy-Item "GMEPDesignTool.zip" -Destination "$zDriveDestDir\$date-$nextVersion"