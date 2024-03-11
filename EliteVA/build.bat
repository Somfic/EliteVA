taskkill /IM "voiceattack.exe" /F

rmdir /S /Q "D:\SteamLibrary\steamapps\common\VoiceAttack2\Apps\EliteVA"
mkdir "D:\SteamLibrary\steamapps\common\VoiceAttack2\Apps\EliteVA"

dotnet clean
dotnet build -c Release
copy /Y "C:\dev\EliteVA\EliteVA\bin\Release\net7.0\" "D:\SteamLibrary\steamapps\common\VoiceAttack2\Apps\EliteVA\"

start "" "D:\SteamLibrary\steamapps\common\VoiceAttack2\VoiceAttack.exe"