---
name: release
description: Prepare and publish a versioned release — version bumps, changelog, build, commit, tag, push
disable-model-invocation: true
argument-hint: "[major|minor|patch] [rc] | [promote|rc]"
---

# Release

Prepare and publish a new release for MyRimWorldMod — either a stable release
or a release candidate.

**Release candidates** (`X.Y.Z-rc.N`) are private test builds: a tagged GitHub
prerelease whose zip can be tried on another machine without building from
source. They never go to the Steam Workshop and get no `CHANGELOG.md` section.
Otherwise an RC is held to the same bar as a stable release (it should be what
would ship), so it runs every step below except the changelog. SemVer orders
`1.3.0 < 1.4.0-rc.1 < 1.4.0-rc.2 < 1.4.0`, so candidates sit between stable
versions without disturbing them.

`$ARGUMENTS` is optional and resolved at step 4, where the version is first
needed. From a stable version: a bump type (`major`, `minor`, `patch`),
optionally followed by `rc`. From an RC version: `promote` (to the stable
version) or `rc` (the next candidate). Ask for whatever is missing.

"The last stable tag" below means the newest tag with no prerelease suffix —
`git describe --tags --abbrev=0 --exclude '*-*'`. Every range in this skill
measures from it, never from an RC tag, so a stable release's changelog covers
everything since the previous stable release.

> **Template note:** this is the trimmed template version of the ecosystem's release
> skill. Released sibling mods (e.g. UniqueMeleeWeapons) insert three more gates
> between build and version-bump once the machinery exists: a translation-expectations
> refresh + checker run (`l10n/` submodule + `Scripts/` shims), a Steam Workshop
> description sync (`.steamworkshop/Description/<Language>.txt`), and a startup smoke
> test (`Scripts/integration-smoke-test.py`). When this mod grows translations or a
> Workshop page, port those steps back from a sibling's release skill, along with its
> RC-promotion rule: the translation refresh runs even when promoting an unchanged
> candidate, since its inputs (an upstream l10n release, a vanilla update) move
> without a commit here.

## Current state

!`grep -o '<modVersion>[^<]*' About/About.xml | sed 's/<modVersion>/About.xml version: /'`
!`git describe --tags --abbrev=0 --exclude '*-*' 2>/dev/null || echo "no stable tags found"`
!`git -c versionsort.suffix=- tag -l 'v*-*' --sort=-v:refname | head -3`
!`git log "$(git describe --tags --abbrev=0 --exclude '*-*' 2>/dev/null || echo 'HEAD~10')..HEAD" --oneline --no-merges`

## Steps

Work through the steps below in order. The release decision — version,
changelog, tag — happens once, at step 4, after everything that can still
change the history. Step 4 is the single release gate; nothing else asks.

**Promoting an RC with nothing committed since its tag** (`git log
<rc-tag>..HEAD` is empty): the candidate already validated this exact tree, so
skip step 3 and go straight to step 4 — say so. Any commit since the RC tag
means the full run.

### 1. Review changes

The commit log since the last stable tag is shown above — read it now to understand
what this release contains. If the repo has no tags yet this is the first
release: use the full history (`git log --oneline --no-merges`) and think in
terms of the mod's shipped feature set rather than a diff. No confirmation —
this is orientation, not a decision.

### 2. Build gate

Run:
```bash
dotnet build MyRimWorldMod.sln -c Release
```

- The csproj sets `TreatWarningsAsErrors`, so the build is also the lint
  gate: any compiler or analyzer warning fails it, and a passing build means
  there is nothing warnings-only left in the log to read out.
- This runs before the clean build and the release commit because it takes
  seconds and fails fast; a broken tree must not reach either.
- On any failure, stop and help the user fix it, then rerun until it passes.
  No confirmation on success.

### 3. Clean build and deploy

Run:
```bash
dotnet clean MyRimWorldMod.sln
dotnet build MyRimWorldMod.sln -c Release
```

Report the build result. If the build fails, stop and help the user fix it.

### 4. Version, changelog, and the single release confirmation

Do all of the following, then present it as **one** confirmation:

- Read the current version from `About/About.xml` (`<modVersion>`) and
  resolve the new version (from `$ARGUMENTS`, or ask now):
  - **Current is stable** (`1.3.0`): apply the bump type, then either stable
    (`1.4.0`) or the first candidate (`1.4.0-rc.1`).
  - **Current is an RC** (`1.4.0-rc.1`): either promote (`1.4.0`) or cut the
    next candidate (`1.4.0-rc.2`). A bump type doesn't apply here; if the
    user gives one anyway, ask what they mean (a different target version
    abandons the current candidate line).
  - Before an RC, confirm its tag doesn't already exist (`git tag -l`).
- **Stable releases only — the changelog.** An RC skips this bullet group
  entirely: no section, no link reference.
  - Draft changelog notes from the full log since the last stable tag,
    grouped by category (Fixes, Features, Polish/Other), omitting
    chore/version-bump commits. When promoting, this spans every candidate:
    a fix for a bug that was introduced and fixed within the candidate line
    never reached users of a stable release, so fold it into the entry it
    corrects or drop it.
  - Each changelog entry is a short one-liner fit for Steam Workshop change
    notes (see the note atop `CHANGELOG.md`).
  - Update `CHANGELOG.md`: new `## [X.Y.Z] - YYYY-MM-DD` section at the top,
    directly below the Keep a Changelog intro paragraph, using today's date
    (this changelog carries no `[Unreleased]` heading; don't add one), plus a
    `[X.Y.Z]: https://github.com/<owner>/<repo>/releases/tag/vX.Y.Z`
    link reference at the bottom, above any older ones.
- Bump the version strings in both files:
  - `About/About.xml` `<modVersion>`: the full version, suffix included
    (`1.4.0-rc.1`). The game treats it as a display-only string, so testers
    with the Workshop copy also subscribed can tell the two apart.
  - `Source/1.6/Properties/AssemblyInfo.cs`: `AssemblyInformationalVersion`
    gets the same full version; `AssemblyVersion` and `AssemblyFileVersion`
    stay four-part numeric `X.Y.Z.0` with no suffix (they can't hold one), so
    they are identical across every candidate and the stable release.
- Show the user, together: current version → new version (and bump type, or
  RC / promotion), the changelog notes (stable only), the full diff of the
  changed files, and exactly what step 5 will do (rebuild, commit
  `chore: Bump version to <version>`, tag `v<version>`, push with tags).
- **Ask the user to confirm — this is the only release confirmation.** On
  edits, apply them and re-show only what changed.

### 5. Rebuild, commit, tag, push

No further questions unless something is unexpected:

- Rebuild (`dotnet build MyRimWorldMod.sln -c Release`) so the deployed
  DLL carries the bumped `AssemblyVersion`. Stop on failure.
- Stage only the release files: `About/About.xml`,
  `Source/1.6/Properties/AssemblyInfo.cs`, and (stable only) `CHANGELOG.md`.
  If other tracked files are modified, list them and ask whether to include
  them (the one conditional exception).
- Commit with message: `chore: Bump version to <version>`
- Tag with: `v<version>`
- Push: `git push && git push --tags`
- Show `git log --oneline -3` and
  `git -c versionsort.suffix=- tag -l 'v*' --sort=-v:refname | head -5` (the
  config puts each candidate *below* its stable; git's default ranks it above).
- **RC:** that's it. The tag-triggered workflow marks any suffixed tag as a
  GitHub prerelease (never "Latest") with a stub body plus the auto-generated
  commit list, and attaches `MyRimWorldMod-v<version>.zip`. Point the user at
  the release page for the zip, and remind them it's publicly downloadable but
  must not be uploaded to the Workshop.
- **Stable:** the **GitHub** release notes need no paste: the tag-triggered
  workflow lifts this version's `CHANGELOG.md` section into the release body
  itself (and hard-fails the release if the section is missing), so the
  changelog entry written at step 4 is the release body.
