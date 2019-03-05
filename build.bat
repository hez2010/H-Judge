@echo off
setlocal enabledelayedexpansion

set vsroot=%programfiles(x86)%\Microsoft Visual Studio

for /f "tokens=*" %%a in ('dir /s /b /q "%vsroot%\VsDevCmd.bat"') do call "%%a" && goto :build

:build
dotnet restore
msbuild hjudge.sln -m -verbosity:m /p:Platform=x64 /p:Configuration=Release