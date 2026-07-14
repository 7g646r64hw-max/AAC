# AAC Release Notes - v0.1.1-alpha.1

Release: `v0.1.1-alpha.1`

## Purpose

This alpha release packages AAC Milestone 1 as a cleaner in-game monitoring
experience without changing the verified runtime architecture or hardware
behavior from the v0.1.0-alpha.1 foundation.

## Highlights

- Flight LCD redesigned for concise pilot information.
- Maintenance LCD redesigned for technician-readable POST status, findings,
  hardware counts, display counts, and event history.
- Engineering LCD redesigned for development/runtime state and milestone status.
- Long LCD strings are split into shorter lines instead of relying on LCD
  wrapping.
- Tagged LCDs are explicitly configured with `TEXT_AND_IMAGE`, `Monospace`, and
  approximately `0.80` font size.
- Startup/status text identifies the version and `MONITOR ONLY` mode.
- Programmable Block `Echo()` mirrors a concise maintenance summary.
- Event log entries now use bracketed tick labels such as `[00052]`.

## Behavior Preserved

The following verified runtime areas are intentionally unchanged:

- HardwareDiscovery logic
- Diagnostics logic
- EventLogger storage behavior
- Runtime update loop cadence
- Monitor-only behavior
- Hardware scanning scope
- Configuration tags

## Programmable Block Package

`src/AAC.cs` is the complete PB-ready script. Paste it directly into a Space
Engineers Programmable Block.

Do not add `using` directives, namespace declarations, or an outer
`Program : MyGridProgram` wrapper. The Space Engineers editor provides the
required wrapper and common references.

## Regression Target

- Compiles with zero errors in the Space Engineers Programmable Block editor.
- Runs without runtime exceptions.
- Updates Flight, Maintenance, and Engineering LCDs when tagged blocks exist.
- Leaves same-construct hardware discovery unchanged.
- Preserves monitor-only operation and does not command ship outputs.
