Compress-Archive -Force -Path "bin", "Symbols", "Install.ps1", "gmep.ico" -DestinationPath "GMEPDesignTool.zip"
Copy-Item "GMEPDesignTool.zip" -Destination "Z:\GMEP Engineers\Users\GMEP Softwares"