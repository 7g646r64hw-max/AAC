# Adaptive Antigravity Controller (AAC)

Project Lead: Nomaddison

AAC is a self-calibrating gravity management system for Space Engineers.

## Current Status

Milestone 1 Foundation (`v0.1.1-alpha.1`)

This release preserves the verified monitor-only runtime shell from
`v0.1.0-alpha.1` and refreshes the pilot, technician, and engineering displays
for easier in-game reading. AAC still discovers same-construct hardware,
evaluates a basic POST, writes role-specific LCD output, and records bounded
maintenance events. Control outputs remain intentionally disabled until
calibration and solver milestones are implemented.

## Implemented Foundation

- AAC Core tick loop running every 100 simulation ticks.
- HardwareDiscovery for ship controllers, gravity generators, artificial mass,
  text panels, alarms, and warning lights.
- Configuration tags for flight, maintenance, and engineering LCDs.
- Diagnostics / POST summary for required gravity-drive hardware, including
  concise findings for missing hardware.
- DisplayManager output for pilot, maintenance, and engineering audiences.
- LCD configuration for `TEXT_AND_IMAGE`, `Monospace`, and approximately `0.80`
  font size.
- Startup banner and display text identifying the current version and
  `MONITOR ONLY` operating mode.
- EventLogger ring buffer for boot and manual rescan events using deterministic,
  bracketed AAC tick labels such as `[00052]` instead of wall-clock time.

## Operator Commands

Run the programmable block with `scan` or `rescan` to force an annotated discovery
pass in the maintenance event log.

## Space Engineers Programmable Block

The file located at `src/AAC.cs` is the **Programmable Block-ready version** of
AAC.

It is intended to be copied directly into a Space Engineers Programmable Block
editor.

Do **not** wrap the file in:

```csharp
public sealed class Program : MyGridProgram
{
    ...
}
```

and do **not** add `using` statements. The programmable block editor provides
these automatically.

If maintaining a standalone development version, generate the PB-ready
`src/AAC.cs` by stripping the outer Program class and using directives.

## Monitor-Only Safety

AAC v0.1.1-alpha.1 does not command gravity generators, artificial mass blocks,
alarms, warning lights, thrusters, gyros, or other control outputs. It only
observes hardware, evaluates diagnostics, writes displays, and echoes a concise
maintenance summary from the programmable block.
