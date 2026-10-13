# NEXUS Velora BSR

**Velora chat song requests for Beat Saber 1.40.8 and 1.44.1**, powered by BeatSaver and SongCore.

> **Development status:** v0.1.0 — working bridge and tested 1.44.1 adapter; 1.40.8 and 1.44.1 request/refresh tested. Not a final public release.

## Supported / planned Beat Saber versions

| Beat Saber version | Status |
| --- | --- |
| **1.44.1 (BSManager legacy)** | Bridge, downloads and live SongCore refresh tested; native request overlay optional (off by default) |
| **1.40.8 (BSManager legacy)** | Bridge, downloads and SongCore refresh acknowledgement tested; overlay optional |
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

## Optional in-game panel (disabled by default)

The in-game panel is **not required** for chat requests, map downloads, or SongCore refreshes. Both supported versions run normally without it.

To opt in, create a plain text file at `<Beat Saber instance>\\Plugins\\NexusVeloraBSR.overlay.cfg` with:

```ini
ShowOverlay=true
```

Restart Beat Saber to apply the setting. Set `ShowOverlay=false` or delete the file to disable the panel again. This is a **per-version** setting: 1.40.8 and 1.44.1 can be configured independently. No rebuild is needed when changing it.

## Optional Velora live chat in VR (experimental)

The **VR-only Velora chat panel** is disabled by default on **1.40.8** and **1.44.1**. It uses independent world-space TextMeshPro message rows without a Unity UI Canvas or Image, not an OBS or desktop overlay. This experimental version intentionally has no background rectangle to avoid the white-box rendering issue. It shows up to eight recent chat messages and does not replay old messages when the bridge first connects. The chat feed is memory-only and limited to 30 messages. This is an experimental first implementation; VR placement and visibility must be verified in-game.

In the same per-instance file `<Beat Saber instance>\\Plugins\\NexusVeloraBSR.overlay.cfg`, add:

```ini
ShowOverlay=false
ShowVeloraChat=true
ChatDuringSongs=false
```

Restart Beat Saber after changing settings. `ShowVeloraChat=false` or removing the setting disables the chat box (the default). Set `ChatDuringSongs=true` to leave the panel visible during gameplay. The old song-request panel (`ShowOverlay`) is independent and remains disabled by default.

Start the bridge as usual; `GET http://127.0.0.1:24842/api/velora/chat` provides the bounded chat feed. The bridge must be restarted to use this endpoint after updating. Existing `!bsr` commands, downloads and SongCore refresh remain separate.

## Viewer commands

- `!bsr <BeatSaver ID or song>` — request a map
- `!queue` — queue status
- `!oops` — remove your latest request
- `!bsrhelp` — help

## Streamer / moderator commands

- `!open`, `!close`, `!skip`, `!remove <ID or position>`, `!clearqueue`
- `!setlimit <1-100>` — set the maximum number of waiting requests per viewer, e.g. `!setlimit 3`
- `!setqueue <1-1000>` — set the maximum total queue length, e.g. `!setqueue 30`
- `!block <ID>` — planned; not fully implemented

## What currently works

The Velora listener polls new chat messages, resolves BeatSaver requests, enforces queue limits, downloads accepted maps to the configured Beat Saber instance and signals the SongCore adapter to refresh custom songs. The 1.44.1 adapter has been tested in-game. The optional native overlay is disabled by default, and its visual layout remains experimental. The bridge logs replies locally; it does **not** yet post responses back into Velora chat.

The bridge listens on `http://127.0.0.1:24842` and exposes development endpoints such as `/api/status` and `/api/queue`.

## Song queue limits

The streamer can configure limits in `src/NexusVeloraBSR.Bridge/appsettings.json` under `NexusVeloraBSR`:

```json
"MaxQueueSize": 50,
"MaxRequestsPerUser": 2
```

Restart the bridge after editing this file. Alternatively, the streamer or a moderator can change either limit **live** through Velora chat with `!setqueue 30` and `!setlimit 3`. Live changes take effect immediately for new requests, do not remove existing queued songs, and **reset to the configured values when the bridge restarts**. A viewer reaching their per-user limit cannot add another song until one of their waiting requests is removed or dequeued. The overall queue limit applies to all viewers combined. The bridge currently logs command responses locally rather than posting them back to Velora chat.

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


### VR chat implementation note

NEXUS Velora Chat uses individual world-space text rows, bounded message history, and a dedicated font fallback. It does not require any third-party chat plugin. The Velora bridge remains the message source; `!bsr` and SongCore refresh are unchanged. Test the VR chat panel on 1.40.8 and 1.44.1 before release.
