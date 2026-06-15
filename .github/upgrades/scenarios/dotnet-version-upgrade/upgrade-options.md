# Upgrade Options — AAV.Mvvm.Ne9.csproj

Assessment: 1 project (net48 → net10.0); old-style csproj; SDK-style conversion required; no incompatible packages; no breaking API changes.

## Strategy

### Upgrade Strategy
Single project with no dependency graph to manage — All-at-Once is the natural fit.

| Value | Description |
|-------|-------------|
| **All-at-Once** (selected) | Upgrade the project in a single atomic pass: convert to SDK-style, change TFM, update packages, and validate. |

## Project Structure

### Project Approach
AAV.Mvvm is a class library with no remaining .NET Framework dependents (StandardLib.csproj already targets net10.0), so a direct in-place TFM replacement is safe.

| Value | Description |
|-------|-------------|
| **In-place** (selected) | Replace the target framework directly. No multi-targeting needed — all consumers are migrating together. |
| Multi-targeting | Add net10.0 alongside net48 to serve both Framework and modern consumers during transition. Not needed here. |

## Modernization

### Nullable Reference Types
Target is net10.0, C# project, and nullable is not currently enabled. Recommended to leave disabled during migration; enable separately as a focused effort after the upgrade completes.

| Value | Description |
|-------|-------------|
| **Leave Disabled** (selected) | Does not enable nullable. Maintains existing null handling. Enable separately after migration as a distinct effort. |
| Enable Nullable Reference Types | Adds `<Nullable>enable</Nullable>` to the project file. Adds compile-time null safety; may require code updates to address warnings. |
