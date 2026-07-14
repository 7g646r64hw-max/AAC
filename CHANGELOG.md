# Changelog

## v0.2.0-alpha.1

- Bumped AAC release references from `v0.1.1-alpha.1` to `v0.2.0-alpha.1`.
- Extended `HardwareSnapshot` with selected controller reference, AAC-owned propulsion counts, and tagged propulsion metadata.
- Added ship-relative coordinate labels for tagged propulsion blocks using the selected ship controller as the reference frame.
- Added a read-only `PhysicsEngineModel` built only from `[AAC]`-tagged gravity generators and artificial mass blocks.
- Added `CapabilityAnalysis` after PEM construction.
- Updated Engineering LCD output with PEM readiness, tagged generator count, tagged artificial mass count, coordinate validity, capability status, and `Control Output: LOCKED`.
- Updated Maintenance LCD output to distinguish detected propulsion hardware from AAC-owned tagged propulsion hardware.
- Preserved Flight LCD philosophy, POST diagnostics behavior, HardwareDiscovery runtime flow, AAC Core update cadence, and monitor-only operation.

## v0.1.1-alpha.1

- Bumped AAC release references from `v0.1.0-alpha.1` to `v0.1.1-alpha.1`.
- Redesigned Flight LCD output for concise pilot status with fixed-width counts and explicit `MONITOR ONLY` startup/status text.
- Redesigned Maintenance LCD output for readable technician diagnostics, POST booleans, hardware counts, display counts, and event history.
- Redesigned Engineering LCD output for development/runtime status without changing discovery, diagnostics, or solver behavior.
- Set AAC LCD surfaces to `TEXT_AND_IMAGE`, `Monospace`, and approximately `0.80` font size.
- Mirrored a concise maintenance status summary to programmable block `Echo()`.
- Reformatted event log entries with bracketed deterministic tick numbers such as `[00052]`.
- Documented the PB-ready `src/AAC.cs` convention: no Program wrapper and no using directives when pasting into a Space Engineers Programmable Block.

## v0.1.0-alpha.1

- Added monitor-only AAC runtime shell.
- Added same-construct hardware discovery.
- Added basic POST diagnostics and role-specific LCD output.
- Added bounded maintenance event logging.
- Documented Milestone 1 foundation behavior and current tags.

## v0.1.0-alpha.0

- Repository bootstrap.
- Initial documentation.
- Planned architecture.
