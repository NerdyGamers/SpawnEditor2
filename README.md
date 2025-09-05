# Spawn Editor

Legacy Windows Forms application for creating Ultima Online spawns.

## Build

1. Install the .NET SDK (8.0 or later).
2. Restore and build the solution:
   ```sh
   dotnet build SpawnEditor.sln
   ```
3. A managed map control has replaced the legacy COM component; no COM registration is required.

## Tests

Run unit tests with:
```sh
dotnet test
```

