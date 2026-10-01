# NEXUS Velora BSR

Velora song-request integration for **Beat Saber 1.42.1** using BeatSaver.

> Development status: **v0.1.0 — foundation / not yet a release build**

Primary channel: `velora.tv/ttvytangelofhype`

## Goal

NEXUS Velora BSR will listen for song requests from Velora chat, validate BeatSaver maps, maintain a protected request queue, expose an OBS-friendly queue display, and pass accepted maps to the Beat Saber side of the integration.

## Viewer commands

- `!bsr <BeatSaver ID or song>` — request a map
- `!queue` — view queue status
- `!oops` — remove your latest request
- `!bsrhelp` — show request help

## Streamer / moderator commands

- `!open`
- `!close`
- `!skip`
- `!remove <ID>`
- `!clearqueue`
- `!block <ID>`

## v0.1.0 foundation

The repository currently contains the shared C# request model, queue engine, command parser, and example configuration. Duplicate-map protection and per-viewer queue limits are implemented in the queue layer.

Next development stages are the BeatSaver client, Velora chat connector, local bridge/API, OBS queue page, and Beat Saber 1.42.1 plugin adapter.

## Safety defaults

Automatic map downloads are disabled in the example configuration. Request validation, queue limits, blacklist support, and moderator-only administrative commands will be enforced before the first usable release.


## Running the bridge (development)

NEXUS Velora BSR now includes a self-contained Windows/.NET bridge. It polls new Velora chat messages for the configured channel and routes commands into the BeatSaver-backed request queue.

Requirements: .NET 8 SDK during development.

1. Run `scripts/run-bridge.bat`.
2. Wait for the console to report that the Velora listener is connected.
3. Send a new `!bsr <BeatSaver ID>` message in the configured Velora channel.
4. Check `/api/queue` on the local bridge to confirm the request was accepted.

The listener primes its message watermark on startup so old chat commands are not replayed into a fresh queue.


## Multi-version Beat Saber support

NEXUS keeps the Velora/BeatSaver bridge version-independent and builds the in-game adapter against each selected Beat Saber installation.

Validated profiles are stored in `config/beatsaber-profiles.json`.

- `stable-1.42.1` preserves the Beat Saber 1.42.1 target.
- `latest` is the separately maintained current-PC target.

### Guarded installer

Run `scripts/install-nexus-bsr.bat` and paste the Beat Saber installation folder. NEXUS detects the installed game version before compiling or copying the adapter.

If the detected version has no validated profile, installation stops and existing plugin files are left unchanged. This is intentional: a future Beat Saber update must be validated before NEXUS marks it supported.

Version-specific build artifacts are retained under `dist/plugins/<profile>/`.


## Public configuration

> **CONFIGURATION NOTICE — EDIT AT YOUR OWN RISK**
>
> Make a backup of your configuration before editing it. Replace only documented example values unless you understand the setting. Invalid JSON, renamed keys, missing quotation marks, commas or brackets can prevent NEXUS Velora BSR from starting, connecting to Velora, downloading maps, or communicating with Beat Saber.

The distributed configuration deliberately does **not** contain the developer's personal Velora channel. Before using NEXUS, replace `YOUR_VELORA_CHANNEL_NAME` with your own channel name.

Example only:

```text
Velora stream URL: https://velora.tv/ttvytangelofhype
Channel value:     ttvytangelofhype
```

Do not copy that example unless it is actually your channel. If the placeholder is left unchanged, NEXUS refuses to start the Velora listener and prints a SETUP REQUIRED message instead of connecting to somebody else's stream.
