@echo off
setlocal enabledelayedexpansion

REM Define repository information
set "OWNER=Somfic"
set "REPO=EliteVA"

REM Query GitHub API to get latest release information
set "API_URL=https://api.github.com/repos/%OWNER%/%REPO%/releases/latest"
set "RELEASE_INFO="

REM VoiceAttack registry information
set "REG_KEY=HKEY_CURRENT_USER\Software\VoiceAttack.com\VoiceAttack"
set "REG_ENTRY=installpath"

REM Query the registry key and extract the value of the specified entry
for /f "tokens=2*" %%A in ('reg query "%REG_KEY%" /v "%REG_ENTRY%" 2^>nul ^| findstr /i "%REG_ENTRY%"') do (
    set "VOICEATTACK_PATH=%%B"

    REM the instlalation path should be the base path, then the Apps folder
    SET "INSTALLATION_PATH=!VOICEATTACK_PATH!\Apps"
)

REM Check if the installation path exists
if not exist "%INSTALLATION_PATH%" (
    echo VoiceAttack installation path not found.
    goto :EOF
)

REM Kill VoiceAttack.exe
taskkill /IM VoiceAttack.exe /F

REM Extract download URL of the zip file from the response
for /f "tokens=2 delims=, " %%A in ('curl -s "%API_URL%" ^| findstr /C:"browser_download_url"') do (
    set "DOWNLOAD_URL=%%~A"
    echo url: !DOWNLOAD_URL!
    REM Check if the download URL ends with ".zip"
    echo !DOWNLOAD_URL! | findstr /C:".zip" >nul
    if not errorlevel 1 goto :Download
)

echo Failed to extract download URL from the response.
goto :EOF

:Download
echo Downloading from %DOWNLOAD_URL%

REM Download the release zip file
curl -L -o eliteva.zip "%DOWNLOAD_URL%"

REM Unzip the downloaded file into the installation path, overwriting existing files
7z x eliteva.zip -o"%INSTALLATION_PATH%" -y

REM Clean up downloaded zip file
del eliteva.zip

REM Start VoiceAttack.exe
start "" "%VOICEATTACK_PATH%\VoiceAttack.exe"
endlocal