# Lattice.AdapterSdk for C#

The public C# adapter SDK for BepInEx, MelonLoader, SMAPI, and other managed game
adapters. It wraps the connection, heartbeat, and event plumbing so you write against
a small, typed surface instead of the raw channel.

## Build

```sh
dotnet build Lattice.AdapterSdk.csproj -c Release
```

The output is `bin/Release/netstandard2.0/Lattice.AdapterSdk.dll`.

## Primary types

| Type | What it does |
| --- | --- |
| `LatticeAdapter` | Connection, heartbeat, event queue, and typed event dispatch. |
| `AdapterDescriptor` | Game and adapter identity: slot, game version, and capabilities. |
| `LatticeEvent` | A normalized event frame, for advanced or manual sends. |
| `LatticeCapability` | The capability bits Weaver negotiates with your adapter. |

## Using it

Call `LatticeAdapter.Start()` when your plugin loads, then call `PumpMainThread()`
from your engine update loop before you touch any engine APIs. That keeps event
delivery on the thread the game expects.
