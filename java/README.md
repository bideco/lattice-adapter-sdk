# Lattice Adapter SDK for Java

The public Java adapter SDK for Fabric, Forge, NeoForge, and other JVM game adapters.
It wraps the connection, heartbeat, and event plumbing so you write against a small,
typed surface instead of the raw channel.

## Build

```sh
./build.bat
```

The output is `build/libs/lattice-adapter-sdk.jar`.

## Primary types

| Type | What it does |
| --- | --- |
| `LatticeAdapter` | Connection, heartbeat, event queue, and typed event dispatch. |
| `AdapterDescriptor` | Game and adapter identity: slot, game version, and capabilities. |
| `LatticeEvent` | A normalized event frame, for advanced or manual sends. |
| `LatticeCapability` | The capability bits Weaver negotiates with your adapter. |

## Using it

Start the adapter when your mod loads, then call `drainEvents()` from the game or
client thread before you touch any game APIs. That keeps event delivery on the thread
the game expects.
