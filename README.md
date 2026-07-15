# Adaptive Antigravity Controller (AAC)

Project Lead: Nomaddison

AAC is a monitor-only gravity management and engineering visibility system for Space Engineers.

## Current Status

Milestone 3 Physics Engine Model Enhancement (`v0.3.0-alpha.1`)

AAC now treats the Physics Engine Model (PEM) as the authoritative digital twin for AAC-owned propulsion hardware while preserving strict monitor-only behavior. The controller discovers `[AAC]` gravity generators and artificial mass blocks, derives orientation and mount metadata from game data, validates lifecycle state, builds dynamic capability groups, and exposes every engineering display value through read-only DebugManager pages.

## Implemented Foundation

- AAC Core tick loop running every 100 simulation ticks.
- Same-construct discovery for controllers, gravity generators, artificial mass, LCD panels, alarms, and warning lights.
- `[AAC]` ownership filtering for propulsion hardware only.
- PEM metadata for entity ID, custom name, mount position, block orientation, gravity projection axis, distance, enabled state, working state, validation state, and contribution state.
- Deterministic debug identifiers such as `GEN-001` and `MASS-001` generated from sorted entity IDs for engineering inspection.
- Generator lifecycle tracking: discovered, owned, working, validated, and contributing.
- Dynamic capability assessment by translation axis with READY status, contributing generator count, tolerance count, and reasons for non-ready axes.
- Engineering PEM Summary with overall health, reference controller, coordinate frame, detected/tagged/contributing/non-contributing counts, and redundancy per axis.
- Permanent read-only DebugManager summary pages plus dedicated one-component-per-page inspectors.
- Debug commands: `debug on`, `debug off`, `debug pem`, `debug discovery`, `debug capability`, `debug performance`, `debug generators`, `debug mass`, `debug next`, and `debug prev`.
- Monitor-only DisplayManager output for flight, maintenance, and engineering LCD audiences.
- Permanent programmable-block `Echo()` debug status line.
- EventLogger ring buffer for boot and manual rescan events.

## Hardware Ownership Rule

Only gravity generators and artificial mass blocks with `[AAC]` in their Custom Name are AAC-owned propulsion hardware. The PEM and capability analysis consume only those tagged propulsion blocks. Ship controllers are exempt so AAC can establish a reference coordinate frame.

AAC never infers orientation, capability, or ownership from block names other than the explicit `[AAC]` ownership tag. Mount position, block orientation, and gravity projection axis are derived from block and controller world matrices.

## Operator Commands

Run the programmable block with `scan` or `rescan` to force an annotated discovery pass in the maintenance event log.

Debug commands are also supported:

- `debug on`
- `debug off`
- `debug pem`
- `debug discovery`
- `debug capability`
- `debug performance`
- `debug generators`
- `debug mass`
- `debug next`
- `debug prev`

When a generator or mass inspector is active, `debug next` and `debug prev` navigate between components in that inspector. Otherwise they navigate summary pages.

## Space Engineers Programmable Block

The file located at `src/AAC.cs` is the **Programmable Block-ready version** of AAC. Copy it directly into a Space Engineers Programmable Block editor.

Do **not** wrap the file in a `Program : MyGridProgram` class and do **not** add `using` statements. The programmable block editor provides these automatically.

## Monitor-Only Safety

AAC v0.3.0-alpha.1 does not command gravity generators, artificial mass blocks, alarms, warning lights, thrusters, gyros, or other control outputs. It only observes hardware, builds the PEM digital twin, evaluates dynamic capability, writes displays, shows read-only debug pages, and echoes concise status.
