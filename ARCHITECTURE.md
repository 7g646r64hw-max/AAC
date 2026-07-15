# AAC Architecture

Core philosophy:

- The ship designer engineers the hardware.
- AAC understands AAC-owned hardware as an explicit digital twin.
- The pilot receives concise operational information.
- The system adapts rather than requiring a prescribed layout.
- No subsystem may influence ship behavior until it can fully explain its own state through DebugManager.

## Runtime Pipeline

`HardwareDiscovery -> HardwareSnapshot -> PhysicsEngineModel -> CapabilityAnalysis -> DebugManager -> DisplayManager`

`DebugManager` is intentionally read-only. It receives operator commands and selects summary or inspector pages, but it does not alter hardware snapshots, diagnostics, PEM contents, capability results, controller state, or ship blocks.

## Runtime Subsystems

- **AAC Core**: owns the update loop and coordinates discovery, diagnostics, PEM construction, capability analysis, debug command handling, and display rendering.
- **HardwareDiscovery**: scans the same construct as the programmable block and captures measured block data without assuming a fixed ship layout.
- **HardwareSnapshot**: carries detected hardware counts plus measured metadata for AAC-owned propulsion blocks.
- **PhysicsEngineModel (PEM)**: authoritative digital twin of `[AAC]` propulsion hardware. It stores measured data and derived data including mount position, block orientation, gravity projection axis, validation state, contribution state, and deterministic debug identifiers.
- **CapabilityAnalysis**: builds dynamic per-axis capability groups from the PEM, determines READY status, contributing generator counts, tolerance counts, and reasons for non-ready axes.
- **DebugManager**: permanent validation layer for read-only debug mode, summary page navigation, dedicated generator and mass inspectors, and the programmable-block `Echo()` debug status line.
- **Configuration**: centralizes AAC tags used for operator blocks and AAC-owned propulsion blocks.
- **Diagnostics**: performs POST readiness checks while preserving prior display behavior.
- **DisplayManager**: renders flight, maintenance, and engineering LCD views. Engineering LCDs become debug displays when debug mode is enabled.
- **EventLogger**: stores a bounded in-memory event history for maintenance LCDs.

## PEM Data Boundaries

Measured data comes from the game: entity IDs, custom names, block positions, block orientation matrices, enabled state, and working state. Derived data is calculated by AAC: mount position, block orientation label, gravity projection axis, lifecycle validation, contribution state, capability groups, redundancy, and health.

Orientation and capability are never inferred from names. Names only establish `[AAC]` ownership.

## Debug Framework

Supported commands:

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

Summary pages are Overview, Discovery, PEM Summary, Capability Analysis, and Performance Placeholder. `debug generators` and `debug mass` enter dedicated one-component-per-page inspectors; `debug next` and `debug prev` navigate inside the active inspector until another summary command is selected.

## Hardware Ownership

Only gravity generators and artificial mass blocks with `[AAC]` in their Custom Name are owned by AAC. The PEM and future solver/control layers must consume the tagged PEM rather than querying hardware directly. Ship controllers remain exempt so the coordinate frame is always discoverable.

## Planned Subsystems

- CalibrationEngine
- GravitySolver
- GeneratorController
- AlertManager

## Safety Posture

`v0.3.0-alpha.1` is monitor-only. It discovers and reports hardware, builds a read-only PEM digital twin, analyzes capability, and renders read-only debug views, but does not modify gravity generator fields, artificial mass state, alarms, warning lights, or ship motion.
