@echo off
setlocal enabledelayedexpansion
set vcvarspath=
set platformtoolversion=
set vsroot=%programfiles(x86)%\Microsoft Visual Studio
set vcvarsinfix=VC\Auxiliary\Build\vcvars64.bat

for %%a in (
2017\Preview-v141
2017\BuildTools-v141
2017\Commuinty-v141
2017\Professional-v141
2017\Enterprise-v141
2019\Preview-v142
2019\BuildTools-v142
2019\Commuinty-v142
2019\Professional-v142
2019\Enterprise-v142
) do for /f "tokens=1,2 delims=-" %%i in ("%%a") do if exist "%vsroot%\%%i\%vcvarsinfix%" (set "vcvarspath=%vsroot%\%%i\%vcvarsinfix%" & set platformtoolversion=%%j)

call "!vcvarspath!"
set "INCLUDE=%cd%\lib\jsoncpp\include\json;!INCLUDE!"

msbuild hjudgeExecWindows\hjudgeExecWindows.vcxproj /p:useEnv=true /p:Configuration=Release /p:Platform=x64 /p:PlatformToolset=!platformtoolversion!

dotnet publish hjudgeWeb\hjudgeWeb.csproj -c Release