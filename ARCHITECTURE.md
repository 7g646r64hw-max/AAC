# AAC Architecture

Core philosophy:

- The ship designer engineers the hardware.
- The AAC understands the hardware.
- The pilot receives concise operational information.
- The system adapts rather than requiring a prescribed layout.
- No subsystem may influence ship behavior until it can fully explain its own state through DebugManager.

## Runtime Pipeline

`HardwareDiscovery -> HardwareSnapshot -> PhysicsEngineModel -> CapabilityAnalysis -> DebugManager -> DisplayManager`

`DebugManager` is intentionally read-only. It receives operator commands and selects debug pages, but it does not alter hardware snapshots, diagnostics, PEM contents, capability results, or ship blocks.

## Runtime Subsystems

- **AAC Core**: owns the update loop and coordinates discovery, diagnostics, PEM construction, capability analysis, debug command handling, and display rendering.
- **HardwareDiscovery**: scans the same construct as the programmable block and builds a hardware snapshot without assuming a fixed ship layout.
- **HardwareSnapshot**: carries detected hardware counts plus detailed metadata for AAC-owned propulsion blocks.
- **PhysicsEngineModel (PEM)**: authoritative internal representation of the gravity-drive system. It is built only from `[AAC]`-tagged gravity generators and artificial mass blocks, with ship-relative direction metadata derived from the selected ship controller.
- **CapabilityAnalysis**: evaluates PEM readiness and reports capability status without issuing controls.
- **DebugManager**: permanent validation layer for read-only debug mode, page navigation, and the programmable-block `Echo()` debug status line.
- **Configuration**: centralizes AAC tags used for operator blocks and AAC-owned propulsion blocks.
- **Diagnostics**: performs the current POST readiness check while preserving Milestone 1 behavior.
- **DisplayManager**: renders separate flight, maintenance, and engineering LCD views when tagged panels are present. Engineering LCDs become the debug display when debug mode is enabled; flight and maintenance displays remain unchanged.
- **EventLogger**: stores a bounded in-memory event history for maintenance LCDs.

## Debug Framework

Supported commands:

- `debug on`
- `debug off`
- `debug pem`
- `debug discovery`
- `debug capability`
- `debug performance`
- `debug next`
- `debug prev`

Debug pages:

1. Overview
2. Discovery
3. PEM Summary
4. Generator Inspector
5. Artificial Mass Inspector
6. Capability Analysis
7. Performance Placeholder

The programmable-block terminal always includes a debug status line, for example `Debug: OFF` or `Debug: PEM Summary (Page 3/7)`.

## Hardware Ownership

Only gravity generators and artificial mass blocks with `[AAC]` in their Custom Name are owned by AAC. The PEM and future solver/control layers must consume that tagged model rather than querying hardware directly. Ship controllers remain exempt so the coordinate frame is always discoverable.

## Planned Subsystems

- CalibrationEngine
- GravitySolver
- GeneratorController
- AlertManager

## Safety Posture

`v0.2.5-alpha.1` is monitor-only. It discovers and reports hardware, builds a read-only PEM, analyzes capability, and renders read-only debug views, but does not modify gravity generator fields, artificial mass state, alarms, warning lights, or ship motion.
