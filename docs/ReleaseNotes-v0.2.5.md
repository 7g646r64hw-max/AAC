# AAC Release Notes - v0.2.5-alpha.1

Release: `v0.2.5-alpha.1`

## Purpose

This alpha release implements AAC Milestone 2.5 by adding a permanent read-only DebugManager while preserving the verified Milestone 2 Physics Engine Model foundation and monitor-only operation.

## Highlights

- Added a dedicated DebugManager for debug mode state, page selection, and navigation.
- Added debug commands: `debug on`, `debug off`, `debug pem`, `debug discovery`, `debug capability`, `debug performance`, `debug next`, and `debug prev`.
- Engineering LCDs become the DebugManager display when debug mode is enabled.
- Flight and Maintenance LCDs remain unchanged in behavior.
- Programmable-block `Echo()` now always includes the active debug status line.
- PEM summary, generator inspector, and artificial mass inspector pages make AAC-owned propulsion hardware inspectable.
- Capability analysis and discovery have dedicated debug views.
- Performance page is present as a placeholder for future profiling without changing runtime behavior.

## Behavior Preserved

The following verified runtime areas remain intentionally preserved:

- Monitor-only operation
- HardwareDiscovery same-construct scanning flow
- PEM ownership rules for `[AAC]` gravity generators and artificial mass only
- Capability analysis semantics
- Flight display philosophy
- Maintenance event log behavior
- No gravity outputs, artificial mass control, PID loops, or flight control

## Hardware Ownership

The Physics Engine Model remains the authoritative internal representation of all AAC-owned hardware. Only gravity generators and artificial mass blocks with `[AAC]` in their Custom Name are included in the PEM. Ship controllers remain exempt from the ownership filter so AAC can determine the ship reference frame.

## Programmable Block Package

`src/AAC.cs` is the complete PB-ready script. Paste it directly into a Space Engineers Programmable Block.

Do not add `using` directives, namespace declarations, or an outer `Program : MyGridProgram` wrapper. The Space Engineers editor provides the required wrapper and common references.

## Regression Target

- Compiles with zero errors in the Space Engineers Programmable Block editor.
- Runs without runtime exceptions.
- Updates Flight, Maintenance, and Engineering LCDs when tagged blocks exist.
- Displays DebugManager pages on the Engineering LCD when debug mode is enabled.
- Always shows the current debug state in programmable-block `Echo()`.
- Builds a PEM from tagged propulsion blocks only.
- Runs capability analysis after PEM construction.
- Leaves same-construct hardware discovery and monitor-only operation intact.
