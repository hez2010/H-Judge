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

## Database
- [PostgreSQL](https://www.postgresql.org/)

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
1. Build hjudge.Exec.Windows/hjudge.Exec.Linux
2. Build hjudge.WebHost (It will automatically build hjudge.Core because hjudge.Core is a dependecy of hjudge.WebHost): Using .NET Core SDK 3.0.100 or above
    ```
    An example:
    cd hjudge.WebHost/src
    dotnet publish -c Release
    ```
3. Copy the output dll file generated in step 1 to the hjudge.JudgeHost build output folder, and rename the dll to `hjudge.Exec.Windows.dll`/`hjudge.Exec.Linux.dll`.


## Setup Development Environment
1. Export an environment variable 'ASPNETCORE_ENVIRONMENT' with 'Development'

## Run
1. Setting your connection string to a PostgreSQL Database in ./hjudge.WebHost/src/appsettings.json (or appsettings.Development.json for development environment)
2. Migrate and update database
    ```
    cd hjudge.WebHost/src
    dotnet ef database update
    ```
3. Build
4. Run hjudge.JudgeHost (can be skipped if you don't use solution submission in Web)
    ```
    cd hjudge.JudgeHost/src
    dotnet run -c [Debug/Release]
    ```
5. Run hjudge.FileHost (can be skipped if you don't use solution submission in Web)
    ```
    cd hjudge.FileHost/src
    dotnet run -c [Debug/Release]
    ```
6. Run hjudge.WebHost
    ```
    cd hjudge.WebHost/src
    dotnet run -c [Debug/Release]
    ```