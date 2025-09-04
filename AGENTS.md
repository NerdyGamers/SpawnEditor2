# AGENTS

This repository contains a legacy C# Windows Forms application (Spawn Editor) for creating Ultima Online spawns.

## Development workflow
- Work in the default branch; no need for additional branches.
- When modifying source code (`*.cs`, `*.resx`, etc.), run:
  - `dotnet build SpawnEditor.sln` – ensures the project compiles.
- No automated tests are available.
- Pure documentation or comment updates do not require build checks.

## Code style
- Preserve the existing indentation. Top-level blocks start with a tab, and nested blocks use spaces; copy the surrounding style.
- Braces open on the same line as declarations.
- Avoid modern C# features (e.g., `var`, generics, LINQ) that are unsupported by the project's .NET version.

## Assets
- Designer-generated files (`*.resx`) and associated code must stay in sync.
- COM dependencies (e.g., `UOMap.ocx`, `AxUOMAPLib.dll`) are required for building and running the application.

## Next steps
- Consider adding tests or updating the project to a modern .NET version in the future.
