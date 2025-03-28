$shortcut = (New-Object -ComObject Wscript.Shell).CreateShortcut("$([Environment]::GetFolderPath('Desktop'))\GMEPDesignTool.lnk")
$currentDir = Get-Location
$shortcut.TargetPath = "$currentDir\bin\Debug\net8.0-windows\GMEPDesignTool.exe"
$shortcut.IconLocation = "$currentDir\gmep.ico"
$shortcut.Save()