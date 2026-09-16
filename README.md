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
