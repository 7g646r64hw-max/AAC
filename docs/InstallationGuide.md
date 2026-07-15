# Installation Guide

Version: `v0.3.0-alpha.1`

AAC v0.3.0-alpha.1 is a monitor-only Space Engineers Programmable Block script. It discovers hardware, evaluates diagnostics, builds a read-only Physics Engine Model (PEM), updates tagged LCDs, provides read-only debug pages, and echoes a concise maintenance/debug summary. It does not apply propulsion or alert outputs.

## Required Blocks

- Programmable Block
- Ship Controller
- Gravity Generator(s)
- Artificial Mass Block(s)

## Optional Blocks

- Flight LCD
- Maintenance LCD
- Engineering Design LCD
- Alarm
- Warning Lights
- Button Panel

## Programmable Block Script

Copy the complete contents of `src/AAC.cs` directly into the Space Engineers Programmable Block editor.

`src/AAC.cs` is already PB-ready. Do **not** add:

- `using` directives
- an outer `public sealed class Program : MyGridProgram` wrapper
- extra namespace declarations

The Space Engineers Programmable Block editor supplies those pieces. Adding them around `src/AAC.cs` can prevent the script from compiling in-game.

## Current Tagging

Add `[AAC]` to gravity generators and artificial mass blocks that AAC is allowed to own and model. Untagged gravity generators and artificial mass blocks are detected for maintenance visibility but are excluded from the PEM and all future control logic.

Ship controllers do not require `[AAC]`; they are exempt so AAC can always determine the ship reference frame.

Add the base tag `[AAC]` to optional AAC-owned alarm and warning-light block names. LCDs use role-specific tags:

- `[AAC] Flight`
- `[AAC] Maintenance`
- `[AAC] Engineering`

Tagged LCDs are configured by AAC as text-and-image surfaces using the Debug monospace font at approximately `0.75` font size.

## Operator Commands

Run the programmable block with either command to add a manual rescan entry to the maintenance event log:

- `scan`
- `rescan`

Discovery also runs automatically every 100 simulation ticks.

Run the programmable block with debug commands to inspect read-only validation pages on the Engineering LCD:

- `debug on`
- `debug off`
- `debug pem`
- `debug discovery`
- `debug capability`
- `debug performance`
- `debug next`
- `debug prev`

## Expected Displays

- **Flight LCD**: concise pilot view with version, monitor-only mode, POST, controller, detected gravity generator count, detected artificial mass count, tick, and a short finding line.
- **Maintenance LCD**: technician view with POST state, hardware booleans, finding text split across short lines, detected hardware counts, AAC-owned tagged propulsion counts, display counts, and bracketed event ticks such as `[00052]`.
- **Engineering LCD**: normal development view with discovery/diagnostic status, disabled solver state, PEM readiness, tagged generator count, tagged artificial mass count, coordinate validity, capability status, `Control Output: LOCKED`, and discovery snapshot. When debug mode is enabled, this LCD becomes the DebugManager display.
- **Programmable Block Echo**: concise maintenance summary plus a permanent debug status line such as `Debug: OFF` or `Debug: PEM Summary (Page 3/7)`.

## Safety Notes

AAC v0.3.0-alpha.1 preserves monitor-only behavior. It does not command gravity generators, artificial mass blocks, alarms, warning lights, thrusters, gyros, or other ship-control outputs.

Block group naming will be finalized in a later milestone.


## Milestone 3 Debug Commands

Use `debug generators` to inspect one gravity generator per page and `debug mass` to inspect one artificial mass block per page. Within either inspector, `debug next` and `debug prev` move between AAC-owned components. Use `debug pem`, `debug discovery`, `debug capability`, or `debug performance` to return to summary pages.

The Engineering display now includes PEM health, detected/tagged/contributing/non-contributing counts, reference controller, coordinate frame, and per-axis redundancy. These values are traceable through DebugManager inspector and capability pages.

AAC remains monitor-only and must not be connected to any expectation of active propulsion control in this milestone.
