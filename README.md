# Adaptive Antigravity Controller (AAC)

Project Lead: Nomaddison

AAC is a self-calibrating gravity management system for Space Engineers.

## Current Status

Milestone 2 Physics Engine Model (`v0.2.0-alpha.1`)

This release preserves the verified monitor-only Milestone 1 foundation and adds the first authoritative internal Physics Engine Model (PEM). AAC still performs same-construct discovery, POST diagnostics, display updates, deterministic maintenance events, and programmable-block echo output without commanding propulsion or alert hardware.

## Implemented Foundation

- AAC Core tick loop running every 100 simulation ticks.
- HardwareDiscovery for ship controllers, gravity generators, artificial mass, text panels, alarms, and warning lights.
- Detailed hardware metadata for AAC-owned propulsion blocks.
- Ship-relative coordinate labels: Forward, Backward, Left, Right, Up, and Down.
- PhysicsEngineModel built only from gravity generators and artificial mass blocks whose Custom Name includes `[AAC]`.
- CapabilityAnalysis that evaluates PEM readiness after model construction.
- Configuration tags for flight, maintenance, and engineering LCDs.
- Diagnostics / POST summary for discovered required hardware, preserving Milestone 1 display philosophy.
- DisplayManager output for pilot, maintenance, and engineering audiences.
- Engineering LCD status for PEM readiness, tagged propulsion hardware, coordinate validity, capability status, and `Control Output: LOCKED`.
- Startup banner and display text identifying the current version and `MONITOR ONLY` operating mode.
- EventLogger ring buffer for boot and manual rescan events using deterministic, bracketed AAC tick labels such as `[00052]`.

## Hardware Ownership Rule

Only gravity generators and artificial mass blocks with the `[AAC]` tag in their Custom Name are AAC-owned propulsion hardware. Only these tagged propulsion blocks are included in the PEM or any future control logic. Ship controllers are exempt from this ownership filter so AAC can always determine the ship reference frame.

## Operator Commands

Run the programmable block with `scan` or `rescan` to force an annotated discovery pass in the maintenance event log.

## Space Engineers Programmable Block

The file located at `src/AAC.cs` is the **Programmable Block-ready version** of AAC. Copy it directly into a Space Engineers Programmable Block editor.

Do **not** wrap the file in a `Program : MyGridProgram` class and do **not** add `using` statements. The programmable block editor provides these automatically.

## Monitor-Only Safety

AAC v0.2.0-alpha.1 does not command gravity generators, artificial mass blocks, alarms, warning lights, thrusters, gyros, or other control outputs. It only observes hardware, evaluates diagnostics, builds the read-only PEM, analyzes capability, writes displays, and echoes a concise maintenance summary.
