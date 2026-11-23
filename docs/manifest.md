# The Lattice manifest (`lattice-game.toml`)

A manifest tells Weaver how to recognise, validate, and present a game. It never routes
input; that is the adapter's job. Validate yours against
`schemas/lattice-game.schema.json` before you ship it.

## Required fields

| Field | Meaning |
| --- | --- |
| `schema_version` | Always `1` for this contract. |
| `game_id` | `snake_case` from `[a-z0-9_]`, stable and unique (for example `stardew_valley`). |
| `display_name` | The name shown in Weaver. |
| `adapter_type` | One of `fabric_mod`, `forge_mod`, `neoforge_mod`, `bepinex_plugin`, `melonloader_plugin`, `smapi_mod`, `ue4ss_mod`, `browser_extension`, `generic_window`. |

## Optional fields

| Field | Meaning |
| --- | --- |
| `input_contract` | For example `"lattice-v1"`. |
| `launch_strategy` | One of `external`, `managed`, `steam`, `epic`, `custom_launcher`. |
| `process_names` | Executable names used to detect the running game. |
| `supported_versions` | Free-form version strings (for example `"1.6.x"`). |
| `required_files` | Adapter files, relative to the game or instance folder. No absolute paths, drive letters, env vars, or `..`. |
| `capabilities` | What the adapter supports (see the list below). Weaver hides controls a game does not declare, and only sends features the adapter both requested and Weaver approved. |
| `profile_roots` | Where meaningful game state lives. Must begin with an allowed env-root and include a game-specific subfolder. Bare roots are rejected. |
| `[setup]` | `summary`, `requires_admin`, `anti_cheat_risk` (`none`, `low`, `medium`, `high`, `unsupported`). |

Allowed env-roots for `profile_roots`: `%APPDATA%`, `%LOCALAPPDATA%`, `%PROGRAMDATA%`,
`%PROGRAMFILES%`, `%PROGRAMFILES(X86)%`, `%USERPROFILE%`.

## Capabilities

```
keyboard, mouse, virtual_cursor, scroll, middle_click, settings_sync,
live_settings_apply, resourcepacks, mod_configs, hud, profiles, window_layout,
multi_instance_switching
```

### Privileged capabilities

These are gated for untrusted packages: `settings_sync`, `live_settings_apply`,
`mod_configs`, `resourcepacks`, `profiles`, and `window_layout`. They are stripped from
community and dev packages unless the package is officially signed.

## Validation rules (strict)

- Unknown fields are rejected. There are no script, command, registry, or driver fields.
- Unknown `adapter_type` or capability values are rejected.
- A `schema_version` newer than Weaver supports is rejected.
- Unsafe paths (absolute, broad, or traversal) are rejected.
