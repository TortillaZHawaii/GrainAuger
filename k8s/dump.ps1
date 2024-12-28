# URL of the server
$serverUrl = $args[0] -or "http://localhost:8000"

# Directory to save the downloaded files
$downloadDir = $args[1] -or "./downloads"

# Create the download directory if it doesn't exist
if (-not (Test-Path -Path $downloadDir)) {
    New-Item -ItemType Directory -Path $downloadDir | Out-Null
}

# Get the list of files from the server
$fileList = Invoke-WebRequest -Uri $serverUrl | Select-String -Pattern 'href="[^"]*"' | ForEach-Object { $_.Matches.Groups[1].Value }

# Download each file
foreach ($file in $fileList) {
    Write-Host "Downloading $file..."
    $fileUrl = "$serverUrl/$file"
    $outputPath = Join-Path -Path $downloadDir -ChildPath $file
    Invoke-WebRequest -Uri $fileUrl -OutFile $outputPath
    Write-Host "Saved $file to $downloadDir"
}

Write-Host "Download completed."