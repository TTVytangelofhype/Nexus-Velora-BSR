# NEXUS Velora BSR

**Velora chat song requests for Beat Saber 1.40.8 and 1.44.1**, powered by BeatSaver and SongCore.

> **Development status:** v0.1.0 — working bridge and tested 1.44.1 adapter; 1.40.8 compatibility is being tested. Not a final public release.

## Supported / planned Beat Saber versions

| Beat Saber version | Status |
| --- | --- |
| **1.44.1 (BSManager legacy)** | Bridge, downloads and live SongCore refresh tested; native request overlay experimental |
| **1.40.8 (BSManager legacy)** | Build/install profile available; requires compilation and in-game testing |
| 1.42.1 | Preserved compatibility target, **not verified** |
| Latest | Separate experimental profile, **not verified** |

Each game version uses its own BSManager installation and independently compiled plugin DLL. The bridge is shared; do not run two copies on port 24842.

## Quick start (Windows)

1. Install Beat Saber **1.40.8** or **1.44.1** through BSManager, with compatible **BSIPA** and **SongCore**.
2. Download the repository and double-click **`INSTALL-NEXUS-PLUGIN.bat`**. Choose **1** for 1.40.8 or **2** for 1.44.1. This compiles against your selected game's installed assemblies and installs only if compilation succeeds.
3. Edit `src/NexusVeloraBSR.Bridge/appsettings.json`: replace `YOUR_VELORA_CHANNEL_NAME` with your Velora channel. Set `BeatSaberPath` to the **same BSManager instance folder** you launch (for example, `C:\Users\YOUR_USER\BSManager\BSInstances\1.40.8`). If you switch versions, update this path.
4. Double-click **`START-NEXUS-BSR.bat`** to launch the bridge. Then launch the selected Beat Saber version from BSManager. The installed BSIPA plugin loads automatically; no rebuild is required for normal play.

Requires the **.NET SDK** for source builds and running the development bridge. The installer expects BSManager instances under `%USERPROFILE%\BSManager\BSInstances\`.

### Manual build (optional)

```powershell
.\scripts\build-profile.ps1 -Profile stable-1.40.8 -BeatSaberDir "$env:USERPROFILE\BSManager\BSInstances\1.40.8" -Install
```

For 1.44.1 use `stable-1.44.1` and the corresponding `1.44.1` directory. A failed build leaves the existing game plugin unchanged.

## Viewer commands

- `!bsr <BeatSaver ID or song>` — request a map
- `!queue` — queue status
- `!oops` — remove your latest request
- `!bsrhelp` — help

## Streamer / moderator commands

- `!open`, `!close`, `!skip`, `!remove <ID or position>`, `!clearqueue`
- `!block <ID>` — planned; not fully implemented

## What currently works

The Velora listener polls new chat messages, resolves BeatSaver requests, enforces queue limits, downloads accepted maps to the configured Beat Saber instance and signals the SongCore adapter to refresh custom songs. The 1.44.1 adapter has been tested in-game. A native NEXUS queue overlay is in development; its placement and menu/gameplay visibility are still being refined. The bridge logs replies locally; it does **not** yet post responses back into Velora chat.

The bridge listens on `http://127.0.0.1:24842` and exposes development endpoints such as `/api/status` and `/api/queue`.

## Configuration and safety

> **EDIT AT YOUR OWN RISK.** Back up configuration files before changing them. Invalid JSON or an incorrect Beat Saber folder can stop requests or install maps into the wrong game instance.

The distributed configuration contains the placeholder `YOUR_VELORA_CHANNEL_NAME`; set it to **your own channel**, not somebody else's. The public example configuration is at `config/nexus-velora-bsr.example.json`.

Map downloads are enabled by default in the bridge configuration. Requests are subject to queue and duplicate limits. This is development software; do not expose the localhost bridge directly to the internet.

## Project structure

- `src/NexusVeloraBSR.Core` — request and queue logic
- `src/NexusVeloraBSR.Bridge` — Velora listener, BeatSaver integration, local API
- `src/NexusVeloraBSR.BeatSaber` — BSIPA/SongCore game adapter and experimental overlay
- `config/beatsaber-profiles.json` — version-specific build profiles
- `scripts/build-profile.ps1` — guarded build/install helper
- `INSTALL-NEXUS-PLUGIN.bat` — choose and install a game version
- `START-NEXUS-BSR.bat` — one-click bridge startup
