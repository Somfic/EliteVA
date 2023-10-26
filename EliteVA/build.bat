taskkill /IM "voiceattack.exe" /F

rmdir /S /Q "C:\Users\Lucas\VoiceAttack\Apps\EliteVA"
mkdir "C:\Users\Lucas\VoiceAttack\Apps\EliteVA"

dotnet clean
dotnet build -c Release
copy /Y "C:\dev\EliteVA\EliteVA\bin\Release\net7.0\" "C:\Users\Lucas\VoiceAttack\Apps\EliteVA\"

start "" "C:\Users\Lucas\VoiceAttack\VoiceAttack.exe"