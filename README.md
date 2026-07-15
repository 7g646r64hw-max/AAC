# Adaptive Antigravity Controller (AAC)

AAC is a monitor-only Space Engineers Programmable Block script for validating an antigravity-drive hardware layout before any control outputs are enabled.

## Version 0.3.0

Milestone 3 expands the Physics Engine Model (PEM) into the single source of truth for propulsion assessment. Hardware discovery only gathers blocks; derived state such as mount position, block orientation, gravity projection axis, capability groups, READY state, redundancy, and tolerance is cached in the PEM.

## Usage

1. Tag AAC-owned gravity generators, artificial mass blocks, and LCDs with `[AAC]`.
2. Add LCD tags `[AAC] Flight`, `[AAC] Maintenance`, or `[AAC] Engineering` to route pages.
3. Run the Programmable Block with no argument for normal updates, or with `scan`/`rescan` to record a manual scan event.
4. Debug commands: `debug on`, `debug off`, `debug summary`, `debug pem`, `debug capability`, `debug generators`, `debug mass`, `debug next`, and `debug prev`.

## Safety

AAC v0.3.0 remains monitor-only. It does not write gravity generator settings, artificial mass settings, ship-controller state, alarms, or warning lights.
