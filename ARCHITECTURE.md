# AAC Architecture

Core philosophy:

- The ship designer engineers the hardware.
- The AAC understands the hardware.
- The pilot receives concise operational information.
- The system adapts rather than requiring a prescribed layout.

## Runtime Pipeline

`HardwareDiscovery -> HardwareSnapshot -> PhysicsEngineModel -> CapabilityAnalysis -> DisplayManager`

## Runtime Subsystems

- **AAC Core**: owns the update loop and coordinates discovery, diagnostics, PEM construction, capability analysis, and display rendering.
- **HardwareDiscovery**: scans the same construct as the programmable block and builds a hardware snapshot without assuming a fixed ship layout.
- **HardwareSnapshot**: carries detected hardware counts plus detailed metadata for AAC-owned propulsion blocks.
- **PhysicsEngineModel (PEM)**: authoritative internal representation of the gravity-drive system. It is built only from `[AAC]`-tagged gravity generators and artificial mass blocks, with ship-relative direction metadata derived from the selected ship controller.
- **CapabilityAnalysis**: evaluates PEM readiness and reports capability status without issuing controls.
- **Configuration**: centralizes AAC tags used for operator blocks and AAC-owned propulsion blocks.
- **Diagnostics**: performs the current POST readiness check while preserving Milestone 1 behavior.
- **DisplayManager**: renders separate flight, maintenance, and engineering LCD views when tagged panels are present.
- **EventLogger**: stores a bounded in-memory event history for maintenance LCDs.

## Hardware Ownership

Only gravity generators and artificial mass blocks with `[AAC]` in their Custom Name are owned by AAC. The PEM and future solver/control layers must consume that tagged model rather than querying hardware directly. Ship controllers remain exempt so the coordinate frame is always discoverable.

## Planned Subsystems

- CalibrationEngine
- GravitySolver
- GeneratorController
- AlertManager

## Safety Posture

`v0.2.0-alpha.1` is monitor-only. It discovers and reports hardware, builds a read-only PEM, and analyzes capability, but does not modify gravity generator fields, artificial mass state, alarms, warning lights, or ship motion.
