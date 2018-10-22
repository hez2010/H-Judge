# H::Judge

## Description
This is the official repository for [H::Judge](https://hjudge.com).

## Frontend
- [Vue.js](https://vuejs.org/)
- [Vuetify](https://vuetifyjs.com/)

## Backend
- [.NET Core 2.1](https://www.microsoft.com/net)

## Structure
- ./hjudgeWeb:
    > The web host for H::Judge using .NET Core.
- ./hjudgeCore:
    > The core module of H::Judge containing all the necessary configurations and methods for judging a submission.
- ./hjudgeExecWindows:
    > A module on Windows operating system used to run a program and measure time, memory consuming and execution status of the process being created. (It can not be platform-irrelavent so we extracted it from H::Judge and made it a replaceable, flexible and platform-relavent dynamic link library, you can also create a 'hjudgeExecLinux' to do the stuff above on Linux operating system)

## Build
1. Build hjudgeExecWindows: Using MSBuild
2. Build hjudgeWeb (It will automatically build hjudgeCore because hjudgeCore is a dependecy of hjudgeWeb): Using .NET Core SDK 2.1.400 or above
    ```
    A windows build example:
    dotnet publish -r win-x64 -c Release
    ```
3. Copy the output dll file generated in step 1 to the hjudgeWeb build output folder, and rename the dll to 'hjudgeExec.dll'.