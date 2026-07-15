# Adaptive Antigravity Controller (AAC)

AAC is a monitor-only Space Engineers Programmable Block script for validating an antigravity-drive hardware layout before any control outputs are enabled.

## Version 0.3.5

Milestone 3.5 refines the Engineering Console while preserving the verified v0.3.0.1 Hardware Discovery, Physics Engine Model (PEM), Capability Assessment, engineering calculations, and monitor-only behavior. `src/AAC.cs` remains the complete self-contained Programmable Block script.

## Usage

1. Tag AAC-owned gravity generators, artificial mass blocks, and LCDs with `[AAC]`.
2. Add LCD tags `[AAC] Flight`, `[AAC] Maintenance`, or `[AAC] Engineering` to route pages.
3. Run the Programmable Block with no argument for normal updates, or with `scan`/`rescan` to record a manual scan event.
4. Engineering Console commands: `debug disc`, `debug pem`, `debug cap`, `debug gen`, `debug mass`, `debug perf`, `debug next`, `debug prev`, and `debug off`.

## Display Roles

- Flight LCD: concise pilot-facing status and Echo-compatible behavior.
- Maintenance LCD: technician-facing health, counts, readiness, redundancy, and event history.
- Engineering Console LCD: standby Status page when debug is off; interactive engineering sections when a debug section command is active.

## Safety

AAC v0.3.5 remains monitor-only. It does not write gravity generator settings, artificial mass settings, ship-controller state, alarms, or warning lights.
