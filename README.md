# Spawn Editor

Legacy Windows Forms application for creating Ultima Online spawns.

## Build

1. On Windows, run `./build.ps1` to install the .NET SDK and build the solution.
2. A managed map control renders land and static tiles and requires no COM registration.

### Cross-platform builds

Windows Forms targets the Windows desktop stack. On non-Windows hosts, cross-compiling is possible with:

```
dotnet build SpawnEditor.sln -p:EnableWindowsTargeting=true
```

The generated binaries still require Windows to run. This repository includes a GitHub Actions workflow that builds on `windows-latest`.

## Tests

Run unit tests with:
```sh
dotnet test
```

