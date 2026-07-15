# Adaptive Antigravity Controller (AAC)

Project Lead: Nomaddison

AAC is a self-calibrating gravity management system for Space Engineers.

## Current Status

Milestone 2.5 Debug & Validation Framework (`v0.2.5-alpha.1`)

This release preserves the verified monitor-only Milestone 2 foundation and adds a permanent read-only `DebugManager`. AAC still performs same-construct discovery, POST diagnostics, display updates, deterministic maintenance events, PEM construction, capability analysis, and programmable-block echo output without commanding propulsion or alert hardware.

## Implemented Foundation

- AAC Core tick loop running every 100 simulation ticks.
- HardwareDiscovery for ship controllers, gravity generators, artificial mass, text panels, alarms, and warning lights.
- Detailed hardware metadata for AAC-owned propulsion blocks.
- Ship-relative coordinate labels: Forward, Backward, Left, Right, Up, and Down.
- PhysicsEngineModel built only from gravity generators and artificial mass blocks whose Custom Name includes `[AAC]`.
- CapabilityAnalysis that evaluates PEM readiness after model construction.
- Permanent DebugManager that only reads subsystem snapshots and never modifies subsystem state.
- Debug commands: `debug on`, `debug off`, `debug pem`, `debug discovery`, `debug capability`, `debug performance`, `debug next`, and `debug prev`.
- Engineering LCD debug pages for overview, discovery, PEM summary, generator inspection, artificial mass inspection, capability analysis, and a performance placeholder.
- Permanent programmable-block `Echo()` debug status line such as `Debug: OFF` or `Debug: PEM Summary (Page 3/7)`.
- Configuration tags for flight, maintenance, and engineering LCDs.
- Diagnostics / POST summary for discovered required hardware, preserving Milestone 1 display philosophy.
- DisplayManager output for pilot, maintenance, and engineering audiences.
- Engineering LCD status for PEM readiness, tagged propulsion hardware, coordinate validity, capability status, and `Control Output: LOCKED` when debug mode is off.
- Startup banner and display text identifying the current version and `MONITOR ONLY` operating mode.
- EventLogger ring buffer for boot and manual rescan events using deterministic, bracketed AAC tick labels such as `[00052]`.

## Hardware Ownership Rule

Only gravity generators and artificial mass blocks with the `[AAC]` tag in their Custom Name are AAC-owned propulsion hardware. Only these tagged propulsion blocks are included in the PEM or any future control logic. Ship controllers are exempt from this ownership filter so AAC can always determine the ship reference frame.

## Operator Commands

Run the programmable block with `scan` or `rescan` to force an annotated discovery pass in the maintenance event log.

Debug commands are also supported:

- `debug on`
- `debug off`
- `debug pem`
- `debug discovery`
- `debug capability`
- `debug performance`
- `debug next`
- `debug prev`

## Space Engineers Programmable Block

The file located at `src/AAC.cs` is the **Programmable Block-ready version** of AAC. Copy it directly into a Space Engineers Programmable Block editor.

Do **not** wrap the file in a `Program : MyGridProgram` class and do **not** add `using` statements. The programmable block editor provides these automatically.

## Monitor-Only Safety

AAC v0.2.5-alpha.1 does not command gravity generators, artificial mass blocks, alarms, warning lights, thrusters, gyros, or other control outputs. It only observes hardware, evaluates diagnostics, builds the read-only PEM, analyzes capability, writes displays, shows read-only debug pages, and echoes a concise maintenance/debug summary.
