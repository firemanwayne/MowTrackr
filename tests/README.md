# tests

Unit tests, written with xUnit. Each project is named `MowTrackr.<Project>.UnitTests` and its folders mirror the
project it tests, so a test for `src/Client/Services/Foo.cs` lives in `tests/Client/Services/FooTests.cs`.

| Folder | Tests |
| --- | --- |
| `Client/` | The Blazor client: handlers, providers and services, such as the label text parser. Uses fakes from `Fakes/`. |
| `Communications/` | Email templates. |
| `Core/` | The shared constants (roles, routes, password policy). |
| `Identity/` | Identity services and models. Uses the EF Core in-memory provider, NSubstitute and a fake clock (`TimeProvider.Testing`). |

## Running them

```bash
dotnet test MowTrackr.slnx --configuration Release     # everything, as CI does
dotnet test tests/Client                               # one project
dotnet test tests/Client --filter "FullyQualifiedName~LabelTextParser"
```

## Conventions

- Test names follow `MethodName_Scenario_ExpectedResult`.
- Helper methods go at the bottom of the test class.
- Internal types are reached through `InternalsVisibleTo`, not by making production types public.
- Add the project's `GlobalUsings.cs` entries there, not in each file.

CI runs the whole suite on every pull request into `dev` or `main`; the `build` check must pass to merge.
