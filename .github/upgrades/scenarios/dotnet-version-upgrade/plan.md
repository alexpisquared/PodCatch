# .NET Version Upgrade Plan

## Overview

**Target**: AAV.Mvvm.Ne9.csproj — legacy .NET Framework 4.8 class library → net10.0
**Scope**: 1 project, small MVVM library; old-style csproj requiring SDK-style conversion and TFM upgrade

### Selected Strategy
**All-At-Once** — All projects upgraded simultaneously in a single operation.
**Rationale**: 1 project targeting net48, no dependency graph to manage. Single atomic pass is the correct approach.

## Tasks

### 01-prerequisites: Verify .NET 10 SDK and toolchain

Confirm that the .NET 10 SDK is installed on the machine and that any `global.json` file in the repository or project folder is compatible with the target SDK version. If a `global.json` is present and pins a lower SDK version, it must be updated (or the `rollForward` policy adjusted) before the upgrade can proceed. This task is a gate for all subsequent tasks.

**Done when**: `dotnet --version` reports a .NET 10 SDK; any `global.json` in the repo does not block net10.0 builds; no SDK-related build errors.

---

### 02-sdk-conversion: Convert AAV.Mvvm.Ne9.csproj to SDK-style format

The project file uses the legacy MSBuild format (`<Project ToolsVersion="12.0">`), which must be converted to SDK-style (`<Project Sdk="Microsoft.NET.Sdk">`) before the TFM can be changed. This conversion stays on the current target framework (net48) — the TFM change is a separate task to isolate failure modes.

SDK-style conversion eliminates the need for explicit file includes, removes the old-style `<Import>` statements and property group boilerplate, and migrates any `packages.config` NuGet references to `<PackageReference>` items. The project should build cleanly on net48 after this task.

**Done when**: AAV.Mvvm.Ne9.csproj uses `<Project Sdk="Microsoft.NET.Sdk">`; project builds successfully on net48 (`dotnet build`); no `packages.config` file remains.

---

### 03-tfm-upgrade: Upgrade TFM to net10.0 and update packages

With the project now in SDK-style format, change the target framework from `net48` to `net10.0` and update all package references to versions compatible with net10.0. Resolve any compilation errors that arise from the framework change — these may include removed or renamed APIs, namespace changes, or packages that need replacement.

The project is an MVVM library (AAV.Mvvm) — investigate WPF or Windows-specific dependencies if any are present, as these require additional project properties (`<UseWPF>true</UseWPF>`) when targeting net10.0. Fix all build errors until the project compiles cleanly.

**Done when**: `<TargetFramework>net10.0</TargetFramework>` is set; all packages reference versions compatible with net10.0; `dotnet build` succeeds with 0 errors; no warnings introduced by the upgrade remain unaddressed.

---

### 04-validation: Final build and test validation

Build the full solution (not just the upgraded project) to confirm no downstream consumers were broken by the upgrade. Run any existing tests for AAV.Mvvm to verify behavior is preserved. Document any deferred items (e.g., nullable reference types, C# version modernization) as follow-up recommendations.

**Done when**: Solution builds with 0 errors; all existing tests pass (or pre-existing failures are documented); no regressions introduced by the upgrade.
