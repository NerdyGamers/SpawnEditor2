# Spawn Editor

Legacy Windows Forms application for creating Ultima Online spawns.

## Build

1. Install the .NET SDK (8.0 or later).
2. Restore and build the solution:
   ```sh
   dotnet build SpawnEditor.sln
   ```
3. The project depends on `UOMap.ocx` and `AxUOMAPLib.dll`; ensure these COM components are registered on your system.

## Tests

Run unit tests with:
```sh
dotnet test
```

