# The Lattice adapter protocol (conceptual, `lattice-v1`)

This describes the connection lifecycle and event model your adapter sees. The exact
binary framing is wrapped by the SDK libraries, so you code against the event model, not
the bytes.

## Connection lifecycle

1. Weaver opens a per-slot local channel. Your adapter connects as a local adapter for
   its slot.
2. **Handshake.** The adapter reports `protocol_version`, `game_id`, `game_version`,
   `adapter_id`, `adapter_version`, `slot`, `instance_path`, `process_id`, and the
   capabilities it wants.
3. **Response.** Weaver replies with accepted or rejected, the effective slot, active or
   inactive, the supported protocol version, the enabled capabilities (requested
   intersected with approved), and diagnostic hints.
4. **Steady state.** The adapter receives input and lifecycle events and sends heartbeats
   (and, if capable, inventories and state updates).
5. **Active or inactive.** When several instances share a slot, Weaver marks exactly one
   active. Only the active instance should act on input.

## Input events (Weaver to adapter)

`mouse_move` (dx, dy relative), `mouse_button` (button, down or up), `mouse_scroll`
(dx, dy), `key_down` and `key_up` (virtual-key), `char` (codepoint).

## Lifecycle events

`capture_state`, `handshake_ack`, `heartbeat`, `slot_busy`, `slot_active`,
`slot_inactive`, `device_change`.

## Capability-gated events (only if negotiated)

Settings sync, live settings apply, resource packs, mod configs, profiles, inventory
snapshots, file sync, and bulk apply. An adapter that does not declare the matching
capability never receives these.

## Error categories

- **Protocol mismatch.** The adapter's `protocol_version` is not supported by Weaver.
- **Unknown game.** No registered manifest matches the `game_id`.
- **Capability denied.** A requested capability was not declared, or was stripped by the
  trust model.

## Compatibility

`lattice-v1` is stable once published. New optional features arrive through capability
negotiation. Breaking changes bump to `lattice-v2`, and Weaver can support several
protocol versions during a migration.

## What you never receive

License status, trial time, license keys, backend responses, and device-capture
internals. You learn only whether the local Weaver accepted your connection.
