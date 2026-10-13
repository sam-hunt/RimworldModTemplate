# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

RimWorld mod template for version 1.7. Use this as a starting point for new mods.

**1.7-only by design.** The mod ships one `1.7/` content tree and one net10 assembly for the
Waylith runtime, with no 1.6 fallback: 1.7 is the stable mainline, new mods have no 1.6 audience
to serve, and a net10 DLL cannot load on 1.6's Mono anyway. A sibling mod that predates 1.7 keeps
a frozen `1.6/` tree beside its `1.7/` one (see `~/dev/RIMWORLD_1.7_MIGRATION.md`, Phase B); this
template never grows one.

**Where documentation lives:** this file holds only cross-cutting rules and rationale. Per-item
values and decompile-verified call paths belong in the header comment of the file they describe.
When adding or changing something, put the *why* there and only add a line here if it constrains
work in other files.

## Build Commands

```bash
# Build (outputs to 1.7/Assemblies/ AND atomically redeploys to the RimWorld Mods folder)
dotnet build MyRimWorldMod.sln -c Release

# Stage the mod into an arbitrary folder (used by CI; same manifest as the local deploy)
dotnet build Source/1.7/MyRimWorldMod.csproj -c Release \
  -t:StageMod -p:StageDir=/path/to/output/MyRimWorldMod

# Override RimWorld install path
RIMWORLD_PATH="/path/to/RimWorld" dotnet build MyRimWorldMod.sln -c Release
# Or: dotnet build -p:RimWorldPath="/path/to/RimWorld"
```

The build needs the .NET 10 SDK and compiles against the `Krafs.Rimworld.Ref` NuGet package
(pinned to an exact 1.7 build) everywhere, locally and in CI; the local RimWorld install is never
referenced, only deployed to. The install path (auto-detected on Windows/Linux/Mac, or
`RIMWORLD_PATH`) decides only whether a Release build deploys, gated on the game's `Mods/` folder
existing. Debug builds go to the default `bin/` and never deploy — only Release builds touch
`1.7/Assemblies/` and the Mods folder.

**Package pins.** `Krafs.Rimworld.Ref` and `Lib.Harmony` are pinned to exact versions; a pin that
is a pre-release carries a `PRE-RELEASE PIN` marker in the csproj comment and must be bumped to
the final before the pinning branch merges to `main`.

**WSL setup:** `RIMWORLD_PATH` in `~/.bashrc` pointing at the Windows install, e.g.
`/mnt/c/Program Files (x86)/Steam/steamapps/common/RimWorld`.

### Deployment

The repo lives outside the Mods folder; every local Release build redeploys automatically and
atomically.

- **One manifest, one place:** the `_ModFiles` ItemGroup in the `StageMod` target of
  `Source/1.7/MyRimWorldMod.csproj` — see that target's comments for how it globs and what it
  excludes. It is generic over folders, so a new `1.7/` or `Sounds/` needs no build change; only a
  brand-new *file type* does. Local deploy and CI release both call it, so they can't drift.
- **Stop hook (`.claude/hooks/sync-mod.sh`):** rebuilds+redeploys after a Claude turn when
  mod-relevant files changed, logs to `$TMPDIR/<repo-folder-name>-build.log`. Tracked and wired
  by the tracked `.claude/settings.json`, so a mod created from this template gets it for free.
  This repo is the canonical home of the script: it is byte-identical across the mod family and
  derives the solution, project folder and mod name itself, so change it here and copy it
  verbatim into the sibling repos, never per repo. It bails when no RimWorld install is found,
  so CI and contributors without the game are unaffected.

**`.claude/` is only partly gitignored.** `.gitignore` carries `.claude/*` followed by
`!.claude/skills/`, `!.claude/hooks/` and `!.claude/settings.json`, so the skills, the Stop hook
and its wiring are tracked and shared while `settings.local.json` (personal permissions) stays
local per machine. Editing a skill is a committed, team-visible change and must keep in step with
whatever it automates (e.g. `/release` encodes the CHANGELOG layout and the version scheme below).

## Project Structure

```
About/           - Mod metadata (About.xml)
Textures/        - Art (version-independent, loaded via the "/" root; no Common/ root)
1.7/             - RimWorld 1.7 specific content
  Assemblies/    - Compiled DLLs (build output, gitignored)
  Defs/          - XML definitions (ThingDefs, etc.)
  Patches/       - XML patches to modify base game/other mods
Source/1.7/      - C# source code targeting net10.0
LoadFolders.xml  - Tells RimWorld which folders to load per game version
CHANGELOG.md     - Keep a Changelog format; load-bearing for releases (see below)
```

## RimWorld Modding Context

- Target framework: net10.0. RimWorld 1.7 runs on Ludeon's Waylith engine over .NET 10; the
  Unity API surface survives as `UnityStubs.dll` under the same `UnityEngine` namespaces, so
  `using UnityEngine;` code reads as before. The Unity-null rules below still apply: the stub
  `Object` keeps the overloaded `==`, and the Unity analyzers still enforce them (verified, see
  the csproj).
- References the RimWorld API through the `Krafs.Rimworld.Ref` package (see the csproj header),
  whose `UnityEngine.*` assemblies are type-forwarding facades onto UnityStubs, so another mod's
  DLL whose signatures name `UnityEngine.CoreModule` types also resolves at compile time.
- Uses `Verse` namespace for core modding APIs
- `[StaticConstructorOnStartup]` attribute triggers code at game startup
- Mod-shipped textures die on an in-process play-data reload (a main-menu language switch):
  `[StaticConstructorOnStartup]` still belongs on the class (main-thread asset loads), but a static
  texture field would go dead, so re-resolve through a getter with a Unity-null `== null` check
  (never `??`/`??=`). Vanilla-path textures persist and are safe as statics. Def-derived startup
  caches need an idempotent per-load `Run()` fired from a `StaticConstructorOnStartupUtility.CallAll`
  postfix (reference: `~/dev/PersonaWeaponsUnbound/Source/1.6/Core/PWU_Startup.cs` and
  `Source/1.6/Defs/PWU_Textures.cs`).
- Harmony is referenced (`Lib.Harmony` 3.x, compile-only) and bootstrapped in `ModInit.cs`; the
  runtime DLL is the `brrainz.harmony` mod dependency's own `1.7/Assemblies/0Harmony.dll`.
  Harmony 3 type-checks injected parameters (`__instance`, `__result`, ...) against the patched
  method at `PatchAll` time; a mismatch is a startup `HarmonyException` ("Unrelated classes
  cannot describe the same non-null object"), so the boot log is the proof for every patch.
- XML Defs define game objects; Patches modify existing Defs via XPath
- **Warnings are build errors.** The csproj sets `TreatWarningsAsErrors`, so every compiler and
  analyzer warning fails the build. `.editorconfig` severities at `warning` block the build;
  `suggestion` is IDE-only.

**Releases:** run the `/release` skill, or by hand: add the version's `## [X.Y.Z]` section to
`CHANGELOG.md`, bump `About/About.xml` `<modVersion>` and `Source/1.7/Properties/AssemblyInfo.cs`,
then push a `v*.*.*` tag. The GitHub Actions workflow (`.github/workflows/release.yml`) builds,
stages via `StageMod`, lifts the tag's CHANGELOG section into the release body — and **fails the
release if that section is missing**. Release candidates are `X.Y.Z-rc.N` tags: CHANGELOG-less and
Workshop-less, with the suffix only in `modVersion` and `AssemblyInformationalVersion` (the numeric
assembly attributes stay `X.Y.Z.0`); `release.yml` treats any suffixed tag as a prerelease.

## Debugging

1. **Dev Mode:** Settings > Dev Mode > Logging.
2. **Log:** `%USERPROFILE%\AppData\LocalLow\Ludeon Studios\RimWorld by Ludeon Studios\Logs\Player.log`
   (WSL: `/mnt/c/Users/*/AppData/LocalLow/Ludeon Studios/RimWorld by Ludeon Studios/Logs/Player.log`;
   Linux: `~/.config/unity3d/Ludeon Studios/RimWorld by Ludeon Studios/Logs/Player.log`). Every
   entry is stamped `YYYY-MM-DD HH:MM:SS.mmm  LEVEL [Category] message` with indented `at ...`
   stack frames. A `Player.log` one level up is a stale pre-1.7 file; ignore it.
3. **Logging convention:** `Log.Message("[MyRimWorldMod] ...")` — grep the prefix to isolate our
   output.
4. **Inspect the API:** `monodis` for signatures, `ilspycmd -t "Namespace.ClassName"` for method
   bodies, both against the local install's `$RIMWORLD_PATH/Assembly-CSharp.dll` (flat install
   root; source of truth over the Krafs ref package), with `UnityStubs.dll` beside it for
   Unity-facing signatures. `Version.txt` in the same folder names the build; a decompile-verified
   call path in a file header should name the build it was verified on, since unstable builds
   move. The `rimworld-logs` skill covers both.

## Localization and Optional-Content Gating

- `MayRequire`/`MayRequireAnyOf` work on def root nodes and list items, but the DefInjected loader ignores XML attributes entirely. A DefInjected entry for a gated def placed in the main tree loads unconditionally and logs a "found no def named ..." startup error whenever the gating mod/DLC is absent.
- The fix is a folder gate: ship the gated content from a compat load root, loaded via an `IfModActive` entry in `LoadFolders.xml` (commented example there). Two flavours, mirroring the ungated roots: `1.7/Mods/<Mod Name>/` for version-specific content (Defs, and the DefInjected targeting them) and root-level `Mods/<Mod Name>/` for version-independent content (art). The def's `MayRequire` becomes redundant and should be dropped when it moves.
- Compat roots must sit BESIDE the well-known folders, never inside them: anything under `1.7/Defs/**` or `1.7/Languages/**` loads unconditionally at any depth.
- The game gates a DefInjected entry by the load root that CONTAINS it, never by the def it targets. The reverse mistake also bites: a main-tree def's translation placed in a compat root silently vanishes when the gate is closed.
- A compat root's language files must never reuse a main-tree file's language-relative path (`DefInjected/<Type>/<File>.xml`, `Keyed/<File>.xml`): the game dedups language files per mod by that path and silently skips one whole file, in an enumeration order that is not LoadFolders order. Suffix compat-root filenames with the gate's name (`WeaponTraits_Royalty.xml`).
- If the mod grows translations, adopt the ecosystem tooling: the shared `rimworld-l10n` toolkit is consumed as a git submodule at `l10n/` (`git submodule add ../rimworld-l10n.git l10n`, pinned to a release tag), with thin per-repo config shims in `Scripts/` (`check-translations.py`, `refresh-translation-expectations.py`, `integration-smoke-test.py` — copy the `SHIM_TEMPLATE.py` files from the toolkit's `checker/`, `refresh/`, `smoke/` dirs), an `expected-injections.json` sidecar generated by the toolkit's L10nProbe dev mod, and the `translate` skill (copy the shape from BetterTradersGuild). The checker validates all of the above bullets mechanically, including English DefInjected files, and gets wired into the release workflow. The `.steamworkshop/Description/<Language>.txt` convention (Workshop title + BBCode description per language) also comes with it.

## Customization Checklist

When creating a new mod from this template, update:
1. `About/About.xml` - mod name, author, packageId (keep it lowercase), description
2. `Source/1.7/ModInit.cs` - namespace, Harmony id (= packageId), log message
3. `Source/1.7/Properties/AssemblyInfo.cs` - title/description/product, a freshly generated GUID
4. `MyRimWorldMod.sln` - rename file and update project name
5. `Source/1.7/MyRimWorldMod.csproj` - rename file (deploy folder name follows the project name)
6. `.github/workflows/release.yml` - the workflow names the .sln, the csproj, and the staged
   folder/zip (`release/MyRimWorldMod`, `MyRimWorldMod-<tag>.zip`) by the project name; update
   every occurrence, so the zip's folder matches the local deploy folder and the README
7. `.claude/skills/` - swap the `MyRimWorldMod` name/log-prefix in `release` and `rimworld-logs`
8. `.vscode/settings.json` - `dotnet.defaultSolution`
