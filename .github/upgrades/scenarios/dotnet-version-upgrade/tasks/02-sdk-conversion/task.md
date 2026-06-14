# 02-sdk-conversion: Convert AAV.Mvvm.Ne9.csproj to SDK-style format

The project file uses the legacy MSBuild format (`<Project ToolsVersion="12.0">`), which must be converted to SDK-style (`<Project Sdk="Microsoft.NET.Sdk">`) before the TFM can be changed. This conversion stays on the current target framework (net48) — the TFM change is a separate task to isolate failure modes.

SDK-style conversion eliminates the need for explicit file includes, removes the old-style `<Import>` statements and property group boilerplate, and migrates any `packages.config` NuGet references to `<PackageReference>` items. The project should build cleanly on net48 after this task.

**Done when**: AAV.Mvvm.Ne9.csproj uses `<Project Sdk="Microsoft.NET.Sdk">`; project builds successfully on net48 (`dotnet build`); no `packages.config` file remains.
