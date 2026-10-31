# LyricsScraperNET (NuGet library)

Notes for AI coding agents working in this repository. Personal or machine-specific notes go to `CLAUDE.local.md` (git-ignored).

## Build
- Targets `net10.0;net9.0;net8.0`, `LangVersion latest` (C# 14): building needs the .NET 10 SDK.
- `dotnet build --configuration Release`
- Lint (same as CI): `dotnet format --verify-no-changes --exclude *\xunit*\*`. Keep this exclude as is.

## Layout
- Entry point: `LyricsScraperNET/LyricsScraperClient.cs` (`ILyricsScraperClient`), fluent setup in `Extensions/LyricsScraperClientExtensions.cs`, DI in `Configuration/ServiceCollectionExtensions.cs`.
- Providers: `LyricsScraperNET/Providers/<Name>/` (`<Name>Provider`, `<Name>Options`, `<Name>Parser`, `<Name>UriConverter`) on top of `Providers/Abstract/ExternalProviderBase.cs`. Enum: `Providers/Models/ExternalProviderType.cs`. Default priorities: `Common/Constants.cs`.
- Usage examples: `LyricsScraperNET.Client/`.
- Tests: `Tests/LyricsScraperNET.UnitTest` (no network), `Tests/LyricsScraperNET.IntegrationTest` (live sites), shared helpers in `Tests/LyricsScraperNET.TestShared`.

## Tests
- xunit.v3 on Microsoft Testing Platform (`global.json`).
- Unit tests with coverage (same as CI): `dotnet test Tests/LyricsScraperNET.UnitTest --configuration Release -- --coverage --coverage-output-format cobertura`
- Integration tests hit real sites and can fail because of site changes or rate limits. One class at a time: build, then run the test exe from `Tests/LyricsScraperNET.IntegrationTest/bin/Release/net10.0` with native xunit options, e.g. `LyricsScraperNET.IntegrationTest.exe -class "LyricsScraperNET.IntegrationTest.Providers.Genius.GeniusProviderTest"`.
- Mutation check for every fix or new behavior: temporarily break the change (revert the fix, remove a guard, replace a condition), run the tests and make sure at least one test fails, then restore it. If no test fails, either the test is missing or weak, or the code is dead and should be removed. Prefer deterministic tests (gates such as `TaskCompletionSource`, `ManualResetEventSlim`) over wall-clock thresholds; use a safety timeout so a broken implementation fails the test instead of hanging it.
- Mutation check pitfall: a file restored from a backup keeps its old timestamp and MSBuild skips the rebuild, so the binaries keep the mutation. `touch` restored files before the final build. Failed tests in the native runner output are lines `<Name> [FAIL]`; check `Failed:` in the summary.
- HTTP tests: no real network in unit tests. Use a real `HttpClient` over a fake `HttpMessageHandler` (`Tests/LyricsScraperNET.UnitTest/Network/HttpTestHelper.cs`). Gotchas:
  - `SendAsync` is protected, so it is matched by name (`HttpTestHelper.CallToSendAsync`); `MustHaveHappened...` needs `using FakeItEasy;`.
  - `HttpClient` does not check a canceled token before calling the handler, so a fake handler does not throw for it; the clients check the token themselves.
  - The client disposes the request after sending: copy what you assert (`SentRequest`) inside the handler.
  - Castle cannot fake `ILogger<T>` for an internal `T`: use `new Logger<T>(fakeLoggerFactory)` over a fake `ILogger`.
  - `new NetHttpClient(null)` binds to the `ILogger` constructor; cast the argument (`(HttpClient)null`).
  - Inserting lines with `sed` into CRLF files makes `dotnet format` fail with WHITESPACE; fix with `dotnet format whitespace --include <file>`.
- New functionality must be covered by tests. Mocks: FakeItEasy. Assert whole objects / all stored data, verify calls to external clients (`A.CallTo(...).MustHaveHappened...`). Keep the existing test style (Arrange / Act / Assert comments, test data in `lyric_test_data.json` + `Resources/`).

## Code organization
- One top-level type per file, file name = type name, folder = namespace (details: "Code organization" in the knowledge base linked from `CLAUDE.local.md`).
- Interfaces and base classes in `Abstract/` of their feature (`Network/Abstract`, `Providers/Abstract`, `Models/Abstract`), enums and data types in `Models/` of their feature (`Network/Models`, `Providers/Models`), public result and request models in `Models/Responses` and `Models/Requests`.
- Obsoletions: one nested class per obsoletion in `Common/Obsoletions.cs`, a page per diagnostic id in `docs/obsoletions/`.

## Public API rules
- This is a published NuGet package: public API changes are breaking changes for consumers. Follow SemVer.
- `ExternalProviderType` numeric values are part of the contract (consumers map the enum by number). Never renumber; give new members explicit values.
- Removing a provider: delete its code and tests, keep the enum member with `[Obsolete]` (forever, the number is the contract), keep `With<Name>()` as an `[Obsolete]` no-op until the next major version (one new diagnostic id per removed provider, see below), add its section name to `Configuration/RemovedProvidersWarning.cs`, and add a row to "Removed providers" in `docs/providers.md` (version and issue).
- Configuration is supported in two ways and both must keep working: dependency injection (`AddLyricScraperClientService` with `IConfiguration`), and manual setup without DI (`LyricScraperClientConfig` bound from `IConfiguration` with its per-provider option properties, or provider options objects, passed to the providers and `new LyricsScraperClient(config, providers)`). Each provider can be enabled, disabled and prioritized from configuration. The per-provider properties of `LyricScraperClientConfig` are public API even though the client itself does not read them.
- Package Validation (`EnablePackageValidation`, baseline = last release) runs on every `dotnet pack` and fails on API changes against the baseline. An intended breaking change of a major version: regenerate `LyricsScraperNET/CompatibilitySuppressions.xml` with `dotnet pack -p:ApiCompatGenerateSuppressionFile=true`, review the diff, and add the change to the migration guide. After a release, move `PackageValidationBaselineVersion` to it and delete the suppressions of the previous major.
- Before deprecating, making internal or removing anything, check all its uses first: tests, `LyricsScraperNET.Client` (also commented-out examples), docs, README and the consumers (LyricsScraperApi, ConcertBuddy). "The library does not call it" does not mean "unused".
- Obsolete members: every `[Obsolete]` uses a diagnostic id and message from `Common/Obsoletions.cs` (one nested class per id) and `Obsoletions.SharedUrlFormat`; ids `LSNET0001`-`LSNET0999`, next free number, never reused; every id has a page `docs/obsoletions/<id>.md` and a row in `docs/obsoletions/README.md`. `ObsoletionsTests` enforces this. Track the removal in an issue of the next major milestone.

## Conventions
- English only in code, comments, docs and commit messages.
- Branches: `feature/<issue>_<desc>`, `bugfix/<issue>_<desc>`. A pushed tag publishes to NuGet (`publish.sh`, `.github/workflows/cicd.yaml`).
- Pull requests always target `main`. PRs are squash-merged, so start every new branch from the latest `main` and do not keep committing to a branch after its PR is merged.
- Commit messages and PR titles start with all issue numbers they cover, e.g. `#54 #29 Removed Musixmatch and LyricFind providers`.
- Provider code must not bypass bot protection, captchas or other access restrictions of the sites (see "Responsible Use" in `README.md`).
- Do not commit real lyrics or saved site pages as test data (#69).
- Follow existing patterns; do not silently change working configs (CI, lint, coverage).
- Trust code over README when they disagree.
- Skip `bin/ obj/ .vs/ TestResults/` when searching.
