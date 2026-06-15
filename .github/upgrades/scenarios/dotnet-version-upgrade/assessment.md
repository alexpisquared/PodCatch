# Projects and dependencies analysis

This document provides a comprehensive overview of the projects and their dependencies in the context of upgrading to .NETCoreApp,Version=v10.0.

## Table of Contents

- [Executive Summary](#executive-Summary)
  - [Highlevel Metrics](#highlevel-metrics)
  - [Projects Compatibility](#projects-compatibility)
  - [Package Compatibility](#package-compatibility)
  - [API Compatibility](#api-compatibility)
  - [Binding Redirect Configuration](#binding-redirect-configuration)
- [Aggregate NuGet packages details](#aggregate-nuget-packages-details)
- [Top API Migration Challenges](#top-api-migration-challenges)
  - [Technologies and Features](#technologies-and-features)
  - [Most Frequent API Issues](#most-frequent-api-issues)
- [Projects Relationship Graph](#projects-relationship-graph)
- [Project Details](#project-details)

  - [AAV.Mvvm.Ne9.csproj](#aavmvvmne9csproj)
  - [C:\g\AAV.Shared\Src\NetLts\StandardLib\StandardLib.csproj](#c:gaavsharedsrcnetltsstandardlibstandardlibcsproj)


## Executive Summary

### Highlevel Metrics

| Metric | Count | Status |
| :--- | :---: | :--- |
| Total Projects | 3 | 1 require upgrade |
| Total NuGet Packages | 6 | All compatible |
| Total Code Files | 23 |  |
| Total Code Files with Incidents | 1 |  |
| Total Lines of Code | 1305 |  |
| Total Number of Issues | 2 |  |
| Estimated LOC to modify | 0+ | at least 0.0% of codebase |

### Projects Compatibility

| Project | Target Framework | Difficulty | Package Issues | API Issues | Binding Issues | Est. LOC Impact | Description |
| :--- | :---: | :---: | :---: | :---: | :---: | :---: | :--- |
| [AAV.Mvvm.Ne9.csproj](#aavmvvmne9csproj) | net48 | 🟢 Low | 0 | 0 | 0 |  | ClassicClassLibrary, Sdk Style = False |
| [C:\g\AAV.Shared\Src\NetLts\StandardLib\StandardLib.csproj](#c:gaavsharedsrcnetltsstandardlibstandardlibcsproj) | net10.0 | ✅ None | 0 | 0 | 0 |  | ClassLibrary, Sdk Style = True |

### Package Compatibility

| Status | Count | Percentage |
| :--- | :---: | :---: |
| ✅ Compatible | 6 | 100.0% |
| ⚠️ Incompatible | 0 | 0.0% |
| 🔄 Upgrade Recommended | 0 | 0.0% |
| ***Total NuGet Packages*** | ***6*** | ***100%*** |

### API Compatibility

| Category | Count | Impact |
| :--- | :---: | :--- |
| 🔴 Binary Incompatible | 0 | High - Require code changes |
| 🟡 Source Incompatible | 0 | Medium - Needs re-compilation and potential conflicting API error fixing |
| 🔵 Behavioral change | 0 | Low - Behavioral changes that may require testing at runtime |
| ✅ Compatible | 0 |  |
| ***Total APIs Analyzed*** | ***0*** |  |

## Aggregate NuGet packages details

| Package | Current Version | Suggested Version | Projects | Description |
| :--- | :---: | :---: | :--- | :--- |
| CliWrap | 3.10.2 |  | [StandardLib.csproj](#c:gaavsharedsrcnetltsstandardlibstandardlibcsproj) | ✅Compatible |
| Microsoft.Extensions.Logging | 10.0.9 |  | [StandardLib.csproj](#c:gaavsharedsrcnetltsstandardlibstandardlibcsproj) | ✅Compatible |
| Microsoft.VisualStudio.Threading.Analyzers | 17.14.15 |  | [StandardLib.csproj](#c:gaavsharedsrcnetltsstandardlibstandardlibcsproj) | ✅Compatible |
| Serilog | 4.3.1 |  | [StandardLib.csproj](#c:gaavsharedsrcnetltsstandardlibstandardlibcsproj) | ✅Compatible |
| Serilog.AspNetCore | 10.0.0 |  | [StandardLib.csproj](#c:gaavsharedsrcnetltsstandardlibstandardlibcsproj) | ✅Compatible |
| Serilog.Sinks.Debug | 3.0.0 |  | [StandardLib.csproj](#c:gaavsharedsrcnetltsstandardlibstandardlibcsproj) | ✅Compatible |

## Top API Migration Challenges

### Technologies and Features

| Technology | Issues | Percentage | Migration Path |
| :--- | :---: | :---: | :--- |

### Most Frequent API Issues

| API | Count | Percentage | Category |
| :--- | :---: | :---: | :--- |

## Projects Relationship Graph

Legend:
📦 SDK-style project
⚙️ Classic project

```mermaid
flowchart LR
    P1["<b>⚙️&nbsp;AAV.Mvvm.Ne9.csproj</b><br/><small>net48</small>"]
    P2["<b>📦&nbsp;StandardLib.csproj</b><br/><small>net10.0</small>"]
    P1 --> P2
    P2 --> P3
    click P1 "#aavmvvmne9csproj"
    click P2 "#c:gaavsharedsrcnetltsstandardlibstandardlibcsproj"

```

## Project Details

<a id="aavmvvmne9csproj"></a>
### AAV.Mvvm.Ne9.csproj

#### Project Info

- **Current Target Framework:** net48
- **Proposed Target Framework:** net10.0
- **SDK-style**: False
- **Project Kind:** ClassicClassLibrary
- **Dependencies**: 1
- **Dependants**: 0
- **Number of Files**: 4
- **Number of Files with Incidents**: 1
- **Lines of Code**: 288
- **Estimated LOC to modify**: 0+ (at least 0.0% of the project)

#### Dependency Graph

Legend:
📦 SDK-style project
⚙️ Classic project

```mermaid
flowchart TB
    subgraph current["AAV.Mvvm.Ne9.csproj"]
        MAIN["<b>⚙️&nbsp;AAV.Mvvm.Ne9.csproj</b><br/><small>net48</small>"]
        click MAIN "#aavmvvmne9csproj"
    end
    subgraph downstream["Dependencies (1"]
        P2["<b>📦&nbsp;StandardLib.csproj</b><br/><small>net10.0</small>"]
        click P2 "#c:gaavsharedsrcnetltsstandardlibstandardlibcsproj"
    end
    MAIN --> P2

```

### API Compatibility

| Category | Count | Impact |
| :--- | :---: | :--- |
| 🔴 Binary Incompatible | 0 | High - Require code changes |
| 🟡 Source Incompatible | 0 | Medium - Needs re-compilation and potential conflicting API error fixing |
| 🔵 Behavioral change | 0 | Low - Behavioral changes that may require testing at runtime |
| ✅ Compatible | 0 |  |
| ***Total APIs Analyzed*** | ***0*** |  |

<a id="c:gaavsharedsrcnetltsstandardlibstandardlibcsproj"></a>
### C:\g\AAV.Shared\Src\NetLts\StandardLib\StandardLib.csproj

#### Project Info

- **Current Target Framework:** net10.0✅
- **SDK-style**: True
- **Project Kind:** ClassLibrary
- **Dependencies**: 1
- **Dependants**: 1
- **Number of Files**: 19
- **Lines of Code**: 1017
- **Estimated LOC to modify**: 0+ (at least 0.0% of the project)

#### Dependency Graph

Legend:
📦 SDK-style project
⚙️ Classic project

```mermaid
flowchart TB
    subgraph upstream["Dependants (1)"]
        P1["<b>⚙️&nbsp;AAV.Mvvm.Ne9.csproj</b><br/><small>net48</small>"]
        click P1 "#aavmvvmne9csproj"
    end
    subgraph current["StandardLib.csproj"]
        MAIN["<b>📦&nbsp;StandardLib.csproj</b><br/><small>net10.0</small>"]
        click MAIN "#c:gaavsharedsrcnetltsstandardlibstandardlibcsproj"
    end
    subgraph downstream["Dependencies (1"]
    end
    P1 --> MAIN
    MAIN --> P3

```

### API Compatibility

| Category | Count | Impact |
| :--- | :---: | :--- |
| 🔴 Binary Incompatible | 0 | High - Require code changes |
| 🟡 Source Incompatible | 0 | Medium - Needs re-compilation and potential conflicting API error fixing |
| 🔵 Behavioral change | 0 | Low - Behavioral changes that may require testing at runtime |
| ✅ Compatible | 0 |  |
| ***Total APIs Analyzed*** | ***0*** |  |

#### Project Package References

| Package | Type | Current Version | Suggested Version | Description |
| :--- | :---: | :---: | :---: | :--- |
| CliWrap | Explicit | 3.10.2 |  | ✅Compatible |
| Microsoft.Extensions.Logging | Explicit | 10.0.9 |  | ✅Compatible |
| Microsoft.VisualStudio.Threading.Analyzers | Explicit | 17.14.15 |  | ✅Compatible |
| Serilog | Explicit | 4.3.1 |  | ✅Compatible |
| Serilog.AspNetCore | Explicit | 10.0.0 |  | ✅Compatible |
| Serilog.Sinks.Debug | Explicit | 3.0.0 |  | ✅Compatible |

