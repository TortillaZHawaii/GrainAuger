# URL of the server
$serverUrl = $args[0]
Write-Host "Downloading files from $serverUrl..."

# Directory to save the downloaded files
$downloadDir = $args[1]
Write-Host "Files will be saved to $downloadDir..."

# Create the download directory if it doesn't exist
if (-not (Test-Path -Path $downloadDir)) {
    New-Item -ItemType Directory -Path $downloadDir | Out-Null
}

# Get the list of files from the server
$Website = Invoke-WebRequest -Uri $serverUrl
$fileList = $Website.Links.href

# Download each file
foreach ($file in $fileList) {
    Write-Host "Downloading $file..."
    $fileUrl = "$serverUrl/$file"
    $outputPath = Join-Path -Path $downloadDir -ChildPath $file
    Invoke-WebRequest -Uri $fileUrl -OutFile $outputPath
    Write-Host "Saved $file to $downloadDir"
}

Write-Host "Download completed."