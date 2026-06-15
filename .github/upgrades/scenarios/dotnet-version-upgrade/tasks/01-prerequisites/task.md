# 01-prerequisites: Verify .NET 10 SDK and toolchain

Confirm that the .NET 10 SDK is installed on the machine and that any `global.json` file in the repository or project folder is compatible with the target SDK version. If a `global.json` is present and pins a lower SDK version, it must be updated (or the `rollForward` policy adjusted) before the upgrade can proceed. This task is a gate for all subsequent tasks.

**Done when**: `dotnet --version` reports a .NET 10 SDK; any `global.json` in the repo does not block net10.0 builds; no SDK-related build errors.
