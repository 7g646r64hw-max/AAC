# Adaptive Antigravity Controller (AAC)

Project Lead: Nomaddison

AAC is a self-calibrating gravity management system for Space Engineers.

## Current Status
Milestone 1 Foundation (`v0.1.0-alpha.1`)

This milestone replaces the Milestone 0 bootstrap placeholder with a monitor-only
runtime shell. AAC now discovers same-construct hardware, evaluates a basic POST,
writes role-specific LCD output, and records bounded maintenance events. Control
outputs remain intentionally disabled until calibration and solver milestones are
implemented.

## Implemented Foundation
- AAC Core tick loop running every 100 simulation ticks.
- HardwareDiscovery for ship controllers, gravity generators, artificial mass,
  text panels, alarms, and warning lights.
- Configuration tags for flight, maintenance, and engineering LCDs.
- Diagnostics / POST summary for required gravity-drive hardware, including
  concise findings for missing hardware.
- DisplayManager output for pilot, maintenance, and engineering audiences.
- EventLogger ring buffer for boot and manual rescan events using deterministic
  AAC tick labels instead of wall-clock time.

## Operator Commands
Run the programmable block with `scan` or `rescan` to force an annotated discovery
pass in the maintenance event log.


## Space Engineers Programmable Block

The file located at `src/AAC.cs` is the **Programmable Block version** of AAC.

It is intended to be copied directly into a Space Engineers Programmable Block editor.

Do **not** wrap the file in:

```csharp
public sealed class Program : MyGridProgram
{
    ...
}
```

and do **not** add `using` statements. The programmable block editor provides these automatically.

If maintaining a standalone development version, generate the PB-ready `src/AAC.cs` by stripping the outer Program class and using directives.
