# H::Judge

## Description
This is the official repository for [H::Judge](https://hjudge.com).

## Status
| master | develop |
| ------ | ------- |
| [![Build Status](https://dev.azure.com/hez2010/H-Judge/_apis/build/status/H-Judge-CI?branchName=master)](https://dev.azure.com/hez2010/H-Judge/_build/latest?definitionId=5&branchName=master) | [![Build Status](https://dev.azure.com/hez2010/H-Judge/_apis/build/status/H-Judge-CI?branchName=develop)](https://dev.azure.com/hez2010/H-Judge/_build/latest?definitionId=5&branchName=develop) |

**Currently master branch is only being maintained, and all new features are being developed in develop branch.**

## Frontend
- [Vue.js](https://vuejs.org/)
- [Vuetify](https://vuetifyjs.com/)

## Backend
- [.NET Core 2.2](https://www.microsoft.com/net)

## Structure
- ./hjudgeWeb:
    > The web host for H::Judge using .NET Core.
- ./hjudgeCore:
    > The core module of H::Judge containing all the necessary configurations and methods for judging a submission.
- ./hjudgeExecWindows:
    > A module on Windows operating system used to run a program and measure time, memory consuming and execution status of the process being created. (It can not be platform-irrelavent so we extracted it from H::Judge and made it a replaceable, flexible and platform-relavent dynamic link library, you can also create a 'hjudgeExecLinux' to do the stuff above on Linux operating system)

## Build
1. Build hjudgeExecWindows
2. Build hjudgeWeb (It will automatically build hjudgeCore because hjudgeCore is a dependecy of hjudgeWeb): Using .NET Core SDK 2.2.100 or above
    ```
    An example:
    dotnet publish -c Release
    ```
3. Copy the output dll file generated in step 1 to the hjudgeWeb build output folder, and rename the dll to 'hjudgeExec.dll'.

## Run
1. Setting your connection string to a SQL Server Database in ./hjudgeWeb/appsettings.json
2. Migrate and update database
    ```
    cd ./hjudgeWeb
    dotnet ef database update
    ```
3. Run hjudgeWeb.dll
    ```
    cd ./hjudgeWeb/bin/netcoreapp2.2/Release/publish
    dotnet ./hjudgeWeb.dll
    ```

## Debug
1. Set environment variable 'ASPNETCORE_ENVIRONMENT' to 'Development'
2. Start debugging
    ```
    cd ./hjudgeWeb
    dotnet run
    ```
