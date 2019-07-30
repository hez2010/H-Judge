# H::Judge

## Description
This is the official repository for [H::Judge](https://hjudge.com).

## Status
| master | develop |
| ------ | ------- |
| [![Build Status](https://dev.azure.com/hez2010/H-Judge/_apis/build/status/H-Judge-CI?branchName=master)](https://dev.azure.com/hez2010/H-Judge/_build/latest?definitionId=5&branchName=master) | [![Build Status](https://dev.azure.com/hez2010/H-Judge/_apis/build/status/H-Judge-CI?branchName=develop)](https://dev.azure.com/hez2010/H-Judge/_build/latest?definitionId=5&branchName=develop) |

## Frontend
- [React](https://reactjs.org/)
- [Semantic UI React](https://react.semantic-ui.com/)

## Backend
- [.NET Core 3.0](https://www.microsoft.com/net/)

## Package Management Tools
- [Yarn](https://yarnpkg.com/)
- [Nuget](https://www.nuget.org/)

## Projects
- hjudge.WebHost:
    > The web host for H::Judge using .NET Core. 
- hjudge.JudgeHost:
    > The judge host for H::Judge using .NET Core. 
- hjudge.FileHost:
    > The file host for H::Judge using .NET Core. 
- hjudge.Shared:
    > The shared components used by other projects. 
- hjudge.Core:
    > The core module of H::Judge containing all the necessary configurations and methods for judging a submission. 
- hjudge.Exec.Windows:
    > A module on Microsoft Windows operating system used to run a program and measure time, memory consuming and execution status of the process being created. 
- hjudge.Exec.Linux:
    > A module on Linux operating system used to run a program and measure time, memory consuming and execution status of the process being created. 

## Build
1. Build hjudge.Exec.Windows
2. Build hjudge.WebHost (It will automatically build hjudge.Core because hjudge.Core is a dependecy of hjudge.WebHost): Using .NET Core SDK 3.0.100 or above
    ```
    An example:
    cd hjudge.WebHost/src
    dotnet publish -c Release
    ```
3. Copy the output dll file generated in step 1 to the hjudge.WebHost build output folder, and rename the dll to 'hjudge.Exec.dll'.

## Run
1. Setting your connection string to a SQL Server Database in ./hjudge.WebHost/src/appsettings.json
2. Migrate and update database
    ```
    cd hjudge.WebHost/src
    dotnet ef database update
    ```
3. Build
4. Run hjudge.WebHost.dll
    ```
    cd hjudge.WebHost/src/bin/netcoreapp3.0/Release/publish
    dotnet ./hjudge.WebHost.dll
    ```

## Debug
1. Export an environment variable 'ASPNETCORE_ENVIRONMENT' with 'Development'
2. Start debugging
    ```
    cd hjudge.WebHost/src
    dotnet run
    ```
