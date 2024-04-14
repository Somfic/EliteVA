taskkill /IM "voiceattack.exe" /F

rm -r "D:\SteamLibrary\steamapps\common\VoiceAttack\Apps\EliteVA"
mkdir "D:\SteamLibrary\steamapps\common\VoiceAttack\Apps\EliteVA"

rm -r "D:\SteamLibrary\steamapps\common\VoiceAttack\Shared\Assemblies\EliteVA"
mkdir "D:\SteamLibrary\steamapps\common\VoiceAttack\Shared\Assemblies\EliteVA"

dotnet remove package Costura.Fody

dotnet clean
dotnet build -c Release
copy /Y "C:\dev\EliteVA\src-plugin\EliteVA\bin\Release\net472\" "D:\SteamLibrary\steamapps\common\VoiceAttack\Apps\EliteVA\"

dotnet clean
dotnet add package Costura.Fody
dotnet build -c Release

copy /Y "C:\dev\EliteVA\src-plugin\EliteVA\bin\Release\net472\" "D:\SteamLibrary\steamapps\common\VoiceAttack\Apps\EliteVA\"

dotnet remove package Costura.Fody

start "" "D:\SteamLibrary\steamapps\common\VoiceAttack\VoiceAttack.exe"

