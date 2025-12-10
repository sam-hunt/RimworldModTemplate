# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

RimWorld mod template for version 1.6. Use this as a starting point for new mods.

## Build Commands

```bash
# Build the mod (from repo root)
dotnet build MyRimWorldMod.sln

# Build release configuration
dotnet build MyRimWorldMod.sln -c Release

# Build with custom RimWorld path
dotnet build MyRimWorldMod.sln -p:RimWorldPath="/path/to/RimWorld"
```

Output DLL goes to `1.6/Assemblies/`.

## Project Structure

```
About/           - Mod metadata (About.xml)
Common/          - Version-independent assets (Languages, Textures)
1.6/             - RimWorld 1.6 specific content
  Assemblies/    - Compiled DLLs (build output, gitignored)
  Defs/          - XML definitions (ThingDefs, etc.)
  Patches/       - XML patches to modify base game/other mods
Source/1.6/      - C# source code targeting net472
LoadFolders.xml  - Tells RimWorld which folders to load per game version
```

## RimWorld Modding Context

- Target framework: .NET Framework 4.7.2
- References RimWorld assemblies via cross-platform paths in .csproj
- Uses `Verse` namespace for core modding APIs
- `[StaticConstructorOnStartup]` attribute triggers code at game startup
- XML Defs define game objects; Patches modify existing Defs via XPath

## Customization Checklist

When creating a new mod from this template, update:
1. `About/About.xml` - mod name, author, packageId, description
2. `Source/1.6/ModInit.cs` - namespace and log message
3. `MyRimWorldMod.sln` - rename file and update project name
4. `Source/1.6/MyRimWorldMod.csproj` - rename file
