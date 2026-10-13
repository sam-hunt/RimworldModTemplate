---
name: rimworld-logs
description: Find and read RimWorld's Player.log, or disassemble the RimWorld API to inspect vanilla types and method signatures. Use when debugging runtime behaviour, chasing an exception, or checking what a vanilla method actually does.
---

# RimWorld Logs and API Inspection

## Reading the log

1. **Enable RimWorld Dev Mode:** Settings → Dev Mode → Logging
2. **Log locations:**
   - **Windows:** `%USERPROFILE%\AppData\LocalLow\Ludeon Studios\RimWorld by Ludeon Studios\Logs\Player.log`
   - **WSL:** `/mnt/c/Users/*/AppData/LocalLow/Ludeon Studios/RimWorld by Ludeon Studios/Logs/Player.log`
   - **Linux (Steam):** `~/.config/unity3d/Ludeon Studios/RimWorld by Ludeon Studios/Logs/Player.log`

   Every entry is `YYYY-MM-DD HH:MM:SS.mmm  LEVEL [Category] message` followed by indented
   `at ...` stack frames, so filter by level:
   `grep -E '^\S+ \S+ +(ERROR|WARN)' | grep -v '\[Vulkan\]'`. A `Player.log` one level up
   (beside `Logs/`) is a stale file from a pre-1.7 game; ignore it.

Our own messages are prefixed `[MyRimWorldMod]`, so grep for that to isolate them from
vanilla and other mods' output.

## Inspecting the RimWorld API

```bash
cat "$RIMWORLD_PATH/Version.txt"                      # which build you are reading
monodis "$RIMWORLD_PATH/Assembly-CSharp.dll"          # flat install root on 1.7
monodis --typedef "$RIMWORLD_PATH/UnityStubs.dll"     # the Unity-facing types
ilspycmd "$RIMWORLD_PATH/Assembly-CSharp.dll" -t Verse.ModContentPack
```

The local RimWorld installation is the source of truth for API shapes — prefer it over the
`Krafs.Rimworld.Ref` NuGet package the build compiles against. Unstable builds move, so note the
build a call path was verified on. `ilspycmd` 10.1+ decompiles the .NET 10 assembly when you
need method bodies rather than signatures.
