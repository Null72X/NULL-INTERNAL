@echo off
setlocal
title Build
color 0A

echo ================================
echo          BUILD STARTED
echo ================================
echo.

echo Publishing project...
dotnet publish -r win-x64 -c Release --self-contained
echo Publish completed.
echo.

echo ================================
echo         FILE TRANSFER
echo ================================
echo.

set "source1=D:\GAURAV INTERNAL\Client\bin\Release\net7.0-windows\win-x64\publish\Client.dll"
set "destination=D:\GAURAV INTERNAL\REQUIREMENTS DLL"

echo Source: %source1%
echo Destination: %destination%
echo.

copy /Y "%source1%" "%destination%" 
echo File copied.
echo.

echo ================================
echo             DONE
echo ================================
pause
