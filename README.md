# Lattice Adapter SDK

> Teach Lattice a new game, then hand it to the community.

![adapters](https://img.shields.io/badge/adapters-C%23%20%C2%B7%20Java-5b6cff)
![input contract](https://img.shields.io/badge/contract-lattice--v1-2ea043)
![surface](https://img.shields.io/badge/surface-public-blue)

This folder is the open front door to Lattice. Everything in it is safe to publish,
so you can add support for a new game without ever touching Weaver's private internals:
licensing, device capture, and input routing all stay on the other side of the wall.

An adapter is two small pieces: a declarative manifest that tells Weaver how to
recognise and present a game, and an in-game mod or plugin that speaks the Lattice
input contract. Write those two things, zip them up, and any Lattice user can install
your game support in a few clicks.

## What you can build

A Lattice adapter lets one game accept routed input from Weaver, so several players
can share a machine, each on their own set of devices. Depending on the game, your
adapter can also declare richer capabilities: settings sync, resource packs, per-player
profiles, mod configs, and multi-instance switching. You declare only what your game
actually supports, and Weaver hides the rest.

Target runtimes covered by the two libraries here:

- **Managed games (C#):** BepInEx, MelonLoader, SMAPI, and similar.
- **JVM games (Java):** Fabric, Forge, and NeoForge.

## What is in the box

| Path | What it gives you |
| --- | --- |
| `schemas/lattice-game.schema.json` | JSON Schema for `lattice-game.toml`. Validate your manifest against this. |
| `samples/fabric-basic/` | A ready-to-copy manifest for a Minecraft / Fabric adapter. |
| `samples/bepinex-basic/` | A manifest for a Unity / BepInEx adapter. |
| `samples/smapi-basic/` | A manifest for a Stardew Valley / SMAPI adapter. |
| `docs/manifest.md` | Every manifest field, how it is validated, and the full capability list. |
| `docs/protocol.md` | The connection lifecycle and event model, explained conceptually. |
| `csharp/` | `Lattice.AdapterSdk` library source and project for C# adapters. |
| `java/` | `lattice-adapter-sdk` library source and build script for JVM adapters. |

## Quick start

**1. Build the library for your language.**

```sh
# C#  ->  csharp/bin/Release/netstandard2.0/Lattice.AdapterSdk.dll
dotnet build csharp/Lattice.AdapterSdk.csproj -c Release

# Java  ->  java/build/libs/lattice-adapter-sdk.jar
cd java && ./build.bat
```

Both libraries wrap the parts you want wrapped: the connection handshake, heartbeat,
capability declaration, and typed input events. From your code it is a handful of calls.

**2. Write a manifest.** Copy the sample closest to your game and edit it. The fields
are small and the schema catches mistakes early:

```toml
schema_version = 1
game_id        = "example_fabric_game"   # snake_case, stable, unique
display_name   = "Example Fabric Game"
adapter_type   = "fabric_mod"
input_contract = "lattice-v1"

capabilities   = ["keyboard", "mouse", "scroll", "settings_sync", "profiles"]
required_files = ["mods/lattice-example.jar"]
```

**3. Validate it** against `schemas/lattice-game.schema.json` with any JSON Schema
tool before you ship. A manifest that fails the schema will be refused by Weaver, so
it is worth catching now.

**4. Wire up the adapter.** Start the SDK when your mod loads, then pump its queue
from the game thread before you touch engine APIs:

- **C#:** call `LatticeAdapter.Start()`, then `PumpMainThread()` in your update loop.
- **Java:** start the adapter on load, then call `drainEvents()` from the client thread.

**5. Package and install.** See the layout below, zip the folder as
`<name>.lattice-game.zip`, and install it through Weaver's installer. For local
iteration you can drop the folder straight into `%ProgramData%/Lattice/games/<game_id>/`.

## Anatomy of a package

A community package is just a folder:

```
my-game/
  lattice-game.toml      # required, validated against the schema
  lattice-package.toml   # optional: adapter_id, publisher, version, signature
  <adapter files...>     # the mod or plugin your required_files point to
```

## The trust model

Not every package is trusted with everything, and that is on purpose.

- **Official** games are built into Weaver.
- **Community** packages are the ones users install. They route input freely, but the
  privileged capabilities (settings sync, resource packs, profiles, mod configs, window
  layout) stay switched off unless the package is signed by an official key.
- **Developer-mode** packages load from `%ProgramData%/Lattice/dev-games/` and only
  when developer mode is on, so you can test your own work without signing it first.

## What is deliberately not here

Some things are closed, and knowing where the line sits saves you time:

- Weaver's source, licensing, device capture, and routing heuristics stay private.
- The raw binary IPC framing is wrapped by the SDK libraries rather than published as
  a stable contract, so the libraries are the supported way in.

One guarantee worth stating plainly: your adapter only ever learns whether the local
Weaver accepted its connection. It never receives license state, trial time, keys, or
backend responses. You cannot leak what you were never handed.

## Status

The declarative surface (schema, manifests, trust model, installer) is implemented and
live in Weaver today. The two SDK libraries ship as working scaffolds that cover
connection, heartbeat, capability declaration, and typed input events. They do not, by
design, expose Weaver licensing, device capture, routing internals, or backend behaviour.

## Contributing

This devkit is open so the community can build adapters for games we have not reached
yet. Issues and pull requests that improve the schema, the sample manifests, or the two
libraries are welcome. Keep the public surface public: nothing here should depend on
Weaver internals.

## License

This SDK is the open, publishable surface of Lattice and may be used to build and
distribute your own game adapters. The rest of Lattice (Weaver and the closed adapters)
is not covered by this folder.
