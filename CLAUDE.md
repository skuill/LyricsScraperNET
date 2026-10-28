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
- New functionality must be covered by tests. Mocks: FakeItEasy. Assert whole objects / all stored data, verify calls to external clients (`A.CallTo(...).MustHaveHappened...`). Keep the existing test style (Arrange / Act / Assert comments, test data in `lyric_test_data.json` + `Resources/`).

## Public API rules
- This is a published NuGet package: public API changes are breaking changes for consumers. Follow SemVer.
- `ExternalProviderType` numeric values are part of the contract (consumers map the enum by number). Never renumber; give new members explicit values.
- Removing a provider: delete its code and tests, keep the enum member with `[Obsolete]`, keep `With<Name>()` as an `[Obsolete]` no-op, add its section name to `Configuration/RemovedProvidersWarning.cs`, and add a row to "Removed Providers" in `README.md` (version and issue).

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
