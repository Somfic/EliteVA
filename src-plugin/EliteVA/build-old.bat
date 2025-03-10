taskkill /IM "voiceattack.exe" /F

rm -r "C:\Program Files\VoiceAttack2\Apps\EliteVA"
mkdir "C:\Program Files\VoiceAttack2\Apps\EliteVA"

rm -r "C:\Program Files\VoiceAttack2\Shared\Assemblies\EliteVA"
mkdir "C:\Program Files\VoiceAttack2\Shared\Assemblies\EliteVA"

dotnet remove package Costura.Fody

dotnet clean
dotnet build -c Release
copy /Y "C:\Users\Lucas\dev\EliteVA\src-plugin\EliteVA\bin\Release\net8.0\" "C:\Program Files\VoiceAttack2\Apps\EliteVA\"

dotnet clean
dotnet add package Costura.Fody
dotnet build -c Release

copy /Y "C:\Users\Lucas\dev\EliteVA\src-plugin\EliteVA\bin\Release\net8.0\" "C:\Program Files\VoiceAttack2\Apps\EliteVA\"

dotnet remove package Costura.Fody

start "" "C:\Program Files\VoiceAttack2\VoiceAttack.exe"