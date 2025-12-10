# RimWorld Mod Template

A ready-to-use template for creating RimWorld 1.6 mods with C# code.

## Quick Start

1. **Use this template** - Click "Use this template" on GitHub or clone/download
2. **Rename your mod** - See [Customization](#customization) below
3. **Set up RimWorld path** - See [Build Setup](#build-setup)
4. **Build** - Run `dotnet build MyRimWorldMod.sln`
5. **Install** - Copy or symlink the mod folder to your RimWorld `Mods/` directory

## Build Setup

The project auto-detects your RimWorld installation on common Steam paths. If your installation is elsewhere, set the `RIMWORLD_PATH` environment variable:

**Windows (PowerShell):**
```powershell
$env:RIMWORLD_PATH = "D:\Games\RimWorld"
dotnet build MyRimWorldMod.sln
```

**Linux/macOS:**
```bash
export RIMWORLD_PATH="$HOME/Games/RimWorld"
dotnet build MyRimWorldMod.sln
```

Or pass it directly:
```bash
dotnet build MyRimWorldMod.sln -p:RimWorldPath="/path/to/RimWorld"
```

### Default Paths

| Platform | Default Path |
|----------|--------------|
| Windows | `C:\Program Files (x86)\Steam\steamapps\common\RimWorld` |
| Linux | `~/.local/share/Steam/steamapps/common/RimWorld` |
| macOS | `~/Library/Application Support/Steam/steamapps/common/RimWorld` |

## Customization

When creating a new mod from this template, rename these files and update their contents:

| File | What to Change |
|------|----------------|
| `About/About.xml` | `<name>`, `<author>`, `<packageId>`, `<description>` |
| `Source/1.6/ModInit.cs` | Namespace, log message |
| `MyRimWorldMod.sln` | Rename file, update project name inside |
| `Source/1.6/MyRimWorldMod.csproj` | Rename file |

### Package ID Format

Use the format `AuthorName.ModName` (e.g., `JohnDoe.CoolMod`). This must be unique across all RimWorld mods.

## Project Structure

```
About/              - Mod metadata (About.xml)
Common/             - Version-independent assets (Languages, Textures)
1.6/                - RimWorld 1.6 specific content
  Assemblies/       - Compiled DLLs (build output)
  Defs/             - XML definitions (ThingDefs, etc.)
  Patches/          - XML patches to modify base game/other mods
Source/1.6/         - C# source code
LoadFolders.xml     - Tells RimWorld which folders to load per game version
```

## Adding Dependencies

To require a DLC or another mod, add to `About/About.xml`:

```xml
<modDependencies>
    <li>
        <packageId>Ludeon.RimWorld.Biotech</packageId>
        <displayName>Biotech</displayName>
    </li>
</modDependencies>
<loadAfter>
    <li>Ludeon.RimWorld.Biotech</li>
</loadAfter>
```

Common DLC package IDs:
- `Ludeon.RimWorld.Royalty`
- `Ludeon.RimWorld.Ideology`
- `Ludeon.RimWorld.Biotech`
- `Ludeon.RimWorld.Anomaly`

## Requirements

- .NET SDK (for building)
- RimWorld 1.6 (for assembly references)

## License

[Choose a license for your mod]
