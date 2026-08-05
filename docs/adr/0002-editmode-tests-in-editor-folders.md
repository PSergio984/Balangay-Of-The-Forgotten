# EditMode tests live in `Editor/` folders; the project uses no asmdefs

The Unity Test Runner was discovering zero tests even though test files existed under `Assets/Scripts/*/Tests/`. The project has no `.asmdef` files at all, so everything compiles into the predefined `Assembly-CSharp`, which the Test Runner does not scan. Tests were relocated under `Editor/` folders (`Assets/Scripts/Leaderboard/Editor/Tests/`, `Assets/Scripts/RewardsManager/Editor/Tests/`), which compiles them into `Assembly-CSharp-Editor` — a predefined assembly the Test Runner discovers for EditMode.

**Status**: accepted

**Considered Options**:
- *Test asmdefs* (`Leaderboard.Tests.asmdef` with "Test Assemblies" checked): rejected — an asmdef cannot reference the predefined `Assembly-CSharp`, so every runtime script the tests touch would also need an asmdef. That is a project-wide refactor with no immediate benefit.
- *Editor folders* (chosen): surgical; only the test files move. `Assembly-CSharp-Editor` auto-references `Assembly-CSharp`, so tests reach all runtime types without changes.

**Consequences**: `.meta` GUIDs are preserved by moving files with their metas; Unity regenerates csproj on next import. Tests compile only into the editor assembly, so they can never leak into builds. If the project adopts asmdefs later (runtime or test), these folders migrate then. The 10 failing tests surfaced by the first run (LeaderboardUITests row counts, NameEntryUITests NREs, VictoryDefeatIntegrationTests) predate this change and were invisible while discovery was broken.
