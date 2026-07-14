# AAC Release Notes - v0.2.0-alpha.1

Release: `v0.2.0-alpha.1`

## Purpose

This alpha release implements AAC Milestone 2 by adding the first monitor-only Physics Engine Model (PEM) while preserving the verified Milestone 1 runtime foundation.

## Highlights

- Hardware snapshots now include detailed metadata for AAC-owned propulsion hardware.
- The selected ship controller defines a ship-relative coordinate frame: Forward, Backward, Left, Right, Up, and Down.
- The PEM is built only from `[AAC]`-tagged gravity generators and artificial mass blocks.
- Capability analysis runs after PEM construction and reports readiness without commanding hardware.
- Engineering LCDs report live PEM readiness, tagged propulsion counts, coordinate validity, capability status, and `Control Output: LOCKED`.
- Maintenance LCDs distinguish detected propulsion blocks from AAC-owned tagged propulsion blocks.

## Behavior Preserved

The following verified runtime areas remain intentionally preserved:

- Monitor-only operation
- HardwareDiscovery same-construct scanning flow
- Diagnostics philosophy and POST behavior
- AAC Core update loop cadence
- Flight display philosophy
- No gravity outputs, artificial mass control, PID loops, or flight control

## Hardware Ownership

Only gravity generators and artificial mass blocks with `[AAC]` in their Custom Name are included in the PEM. Ship controllers remain exempt from the ownership filter so AAC can determine the ship reference frame.

## Programmable Block Package

`src/AAC.cs` is the complete PB-ready script. Paste it directly into a Space Engineers Programmable Block.

Do not add `using` directives, namespace declarations, or an outer `Program : MyGridProgram` wrapper. The Space Engineers editor provides the required wrapper and common references.

## Regression Target

- Compiles with zero errors in the Space Engineers Programmable Block editor.
- Runs without runtime exceptions.
- Updates Flight, Maintenance, and Engineering LCDs when tagged blocks exist.
- Builds a PEM from tagged propulsion blocks only.
- Runs capability analysis after PEM construction.
- Leaves same-construct hardware discovery and monitor-only operation intact.
