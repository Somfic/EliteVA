Write-Host "Downloading and installing the latest release of EliteVA..."    

# Define repository information
$OWNER = "Somfic"
$REPO = "EliteVA"

# Query GitHub API to get latest release information
$API_URL = "https://api.github.com/repos/$OWNER/$REPO/releases/latest"
$RELEASE_INFO = ""

# VoiceAttack registry information
$REG_KEY = "HKCU:\Software\VoiceAttack.com\VoiceAttack"
$REG_ENTRY = "installpath"
$VOICEATTACK_PATH = ""

# Query the registry key and extract the value of the specified entry
$registryValue = Get-ItemProperty -Path $REG_KEY -Name $REG_ENTRY -ErrorAction SilentlyContinue
if ($registryValue) {
    $VOICEATTACK_PATH = $registryValue.installpath
}

# Check if the installation path exists
if (-not (Test-Path $VOICEATTACK_PATH)) {
    Write-Host "VoiceAttack installation path not found."
    exit
}

# Kill VoiceAttack.exe
Stop-Process -Name VoiceAttack -Force -ErrorAction SilentlyContinue

# Query the GitHub API and extract the download URL of the zip file
$response = Invoke-RestMethod -Uri $API_URL
$DOWNLOAD_URL = ($response.assets | Where-Object { $_.name -like "*.zip" }).browser_download_url

if (-not $DOWNLOAD_URL) {
    Write-Host "Failed to extract download URL from the response."
    exit
}

Write-Host "Downloading from $DOWNLOAD_URL"

# Download the release zip file
Invoke-WebRequest -Uri $DOWNLOAD_URL -OutFile "eliteva.zip"

Write-Host "Download complete."

Write-Host "Installing EliteVA..."

# Unzip the downloaded file into the installation path, overwriting existing files
Expand-Archive -Path "eliteva.zip" -DestinationPath $VOICEATTACK_PATH -Force

Write-Host "Installation complete."
 
# Clean up downloaded zip file
Remove-Item -Path "eliteva.zip"

# Start VoiceAttack.exe
Start-Process -FilePath "$VOICEATTACK_PATH\VoiceAttack.exe"
