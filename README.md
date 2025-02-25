# Getting Started

## Environment Setup

- Ensure the SDK for .NET 8 is installed on the machine.
    - You can use the PowerShell script in
      the [linked guide](https://learn.microsoft.com/en-us/dotnet/core/install/windows#install-with-powershell)
- Install the MAUI workload depending on your IDE of choice.
    - Visual Studio: https://learn.microsoft.com/en-us/dotnet/maui/get-started/installation?view=net-maui-9.0&tabs=vswin
    - JetBrains Rider: https://www.jetbrains.com/help/rider/MAUI.html

## Project Setup

```powershell
$repoName = "CoursePlanner"
$owner = "tbui17"
$repoUrl = "https://github.com/$owner/$repoName"
git clone $repoUrl
cd $repoName
dotnet build
```

- Use your IDE of choice to run the entry point project `CoursePlanner` with an emulator:
    - Visual Studio: https://learn.microsoft.com/en-us/dotnet/maui/android/emulator/?view=net-maui-9.0
    - JetBrains Rider: https://www.jetbrains.com/help/rider/MAUI.html#run_debug

# Project Architecture

- `CoursePlanner` - Client & UI
- `MauiConfig` Client DI config
- `ViewModels` - Frontend behavior
- `Lib` - Backend oriented code and utilities

# CI/CD

- The `BuildLib` project contains modules to fetch configurations and keys from the organization's Azure Key Vault and
  build the project using them before uploading it to the organization's Google Play project
- The infrastructure code (i.e. Terraform) is in a different repository.