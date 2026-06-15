# .NET Version Upgrade

## Preferences
- **Flow Mode**: Automatic
- **Target Framework**: net10.0

## Source Control
- **Source Branch**: DirectPortTo.Net9
- **Working Branch**: upgrade-dotnet-10
- **Commit Strategy**: Single Commit at End
- **Branch Sync**: Auto (Merge)

## Strategy
**Selected**: All-at-Once
**Rationale**: Single project (AAV.Mvvm.Ne9.csproj), no dependency graph to manage.

### Execution Constraints
- Single atomic upgrade — convert SDK-style, then upgrade TFM, then validate
- SDK-style conversion (task 02) must complete and build cleanly on net48 before TFM upgrade (task 03)
- Fix all build warnings in modified projects before marking tasks complete
- Commit everything at the end as one atomic commit

## Upgrade Options
**Source**: .github/upgrades/scenarios/dotnet-version-upgrade/upgrade-options.md

### Strategy
- Upgrade Strategy: All-at-Once

### Project Structure
- Project Approach: In-place

### Modernization
- Nullable Reference Types: Leave Disabled
