# Installation Guide (Draft)

Required:
- Programmable Block
- Ship Controller
- Gravity Generator(s)
- Artificial Mass Block(s)

Optional:
- Flight LCD
- Maintenance LCD
- Engineering Design LCD
- Alarm
- Warning Lights
- Button Panel

## Current Tagging

Add the base tag `[AAC]` to optional AAC-owned alarm and warning-light block
names. LCDs use role-specific tags:

- `[AAC] Flight`
- `[AAC] Maintenance`
- `[AAC] Engineering`

The programmable block may be run with `scan` or `rescan` to add a manual rescan
entry to the maintenance event log. Discovery still runs automatically every 100
simulation ticks.

Block group naming will be finalized in a later milestone.
