# TODO

- [ ] Migrate project to modern .NET (e.g., .NET 6)
  - [x] Convert `.csproj` to SDK-style and run `dotnet build` (build currently fails: missing `System.Resources.Extensions`).
  - Next: Resolve build errors and update `TargetFramework` to `net6.0-windows`.
- [ ] Replace COM dependencies with managed alternatives
  - Next: Research libraries to replace `UOMap.ocx` and `AxUOMAPLib.dll` (network access blocked; needs follow-up).
- [ ] Add automated unit tests
  - [x] Choose a test framework (NUnit) and set up a test project.
  - Next: Write tests covering existing functionality.
- [ ] Introduce continuous integration
  - [x] Create a GitHub Actions workflow to run `dotnet build`.
  - Next: Ensure workflow passes once project builds successfully.
- [ ] Improve developer documentation
  - [x] Document build and setup steps in `README.md`.
  - Next: Expand usage examples and contribution guidelines.
