# 03-tfm-upgrade: Upgrade TFM to net10.0 and update packages

With the project now in SDK-style format, change the target framework from `net48` to `net10.0` and update all package references to versions compatible with net10.0. Resolve any compilation errors that arise from the framework change — these may include removed or renamed APIs, namespace changes, or packages that need replacement.

The project is an MVVM library (AAV.Mvvm) — investigate WPF or Windows-specific dependencies if any are present, as these require additional project properties (`<UseWPF>true</UseWPF>`) when targeting net10.0. Fix all build errors until the project compiles cleanly.

**Done when**: `<TargetFramework>net10.0</TargetFramework>` is set; all packages reference versions compatible with net10.0; `dotnet build` succeeds with 0 errors; no warnings introduced by the upgrade remain unaddressed.
