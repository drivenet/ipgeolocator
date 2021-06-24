@echo off
rmdir /s /q packages\linux-x64\ipgeolocator
mkdir packages\linux-x64\ipgeolocator
dotnet publish IpGeolocator --force --output packages\linux-x64\ipgeolocator -c Integration -r linux-x64 --self-contained false
move packages\linux-x64\ipgeolocator\Microsoft.Extensions.Hosting.Systemd.dll "%TEMP%"
del packages\linux-x64\ipgeolocator\web.config packages\linux-x64\ipgeolocator\*.deps.json packages\linux-x64\ipgeolocator\Microsoft.*.dll
move "%TEMP%\Microsoft.Extensions.Hosting.Systemd.dll" packages\linux-x64\ipgeolocator
