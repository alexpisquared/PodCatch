# Task 03-tfm-upgrade — Progress Details

## Summary
TFM upgraded from `net48` (legacy, incorrectly set to `net10` by conversion tool) to `net10.0-windows` (required for WPF). Build succeeds with 0 errors.

## Changes Made

### AAV.Mvvm.Ne9.csproj
- `<TargetFramework>` changed to `net10.0-windows` (WPF requires Windows-specific TFM)
- Removed `<ImportWindowsDesktopTargets>true</ImportWindowsDesktopTargets>` (Framework-only; not needed for .NET 10 WPF)
- Removed empty SCC properties (`SccProjectName`, `SccLocalPath`, `SccAuxPath`, `SccProvider`)
- Removed `<Configurations>Debug;Release;Release64</Configurations>` (not standard in SDK-style)
- Removed platform-specific PropertyGroups with old output paths (x86/x64 Debug/Release) — SDK-style handles these automatically
- Added `<LangVersion>latest</LangVersion>` and `<Nullable>disable</Nullable>` for clean net10.0 defaults
- Assembly references (`System.ComponentModel.DataAnnotations`, `System.Data.DataSetExtensions`, `Microsoft.CSharp`) had already been removed by the SDK-style conversion tool
- All linked source files from `C:\c\AsLink\MVVM\` remain (confirmed to exist on disk)

### Notes
- The `convert_project_to_sdk_style` tool had set TFM to `net10` (interpreted by NuGet as `.NETFramework,Version=v1.0`) — corrected to `net10.0-windows`
- WPF requires the `-windows` platform suffix; `net10.0` alone does not support WPF

## Build Results

```
dotnet build Src\Port.Net9\AAV.Mvvm\AAV.Mvvm.Ne9.csproj

Build succeeded.
  AAV.Mvvm.Ne9 -> ...\bin\Debug\net10.0-windows\AAV.Mvvm.dll

  3 Warning(s) — all in dependency projects (not modified):
    - OpenVsOnTheCulpritLine: CA1416 Console.Beep (pre-existing)
    - StandardLib: CA1416 Console.Beep (pre-existing)
  0 Error(s)
```

AAV.Mvvm itself: 0 warnings, 0 errors ✅

## "Done When" Criteria
- ✅ `<TargetFramework>net10.0-windows</TargetFramework>` set
- ✅ All packages compatible with net10.0 (no packages referenced; project uses only project references and BCL)
- ✅ `dotnet build` succeeds with 0 errors
- ✅ No warnings in modified project (AAV.Mvvm) — pre-existing warnings in unmodified dependency projects noted
