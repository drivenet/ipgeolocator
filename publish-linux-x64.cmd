@echo off
rmdir /s /q packages\linux-x64\ipgeolocator
mkdir packages\linux-x64\ipgeolocator
dotnet publish ipgeolocator --output packages\linux-x64\ipgeolocator -c Release -r linux-x64 --self-contained false
move packages\linux-x64\ipgeolocator\Microsoft.AspNetCore.Server.Kestrel.Transport.Libuv.dll "%TEMP%"
del packages\linux-x64\ipgeolocator\web.config packages\linux-x64\ipgeolocator\*.deps.json packages\linux-x64\ipgeolocator\*settings.json packages\linux-x64\ipgeolocator\Microsoft.*.dll
move "%TEMP%\Microsoft.AspNetCore.Server.Kestrel.Transport.Libuv.dll" packages\linux-x64\ipgeolocator
