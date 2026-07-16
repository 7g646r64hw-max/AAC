# AAC Engineering Console

Milestone 3.5.1 polishes the Engineering Console as the engineer-facing interface for AAC. It is standby when not in a debug section and interactive while a debug section is active.

## Commands

- `debug disc` enters Discovery.
- `debug pem` enters PEM.
- `debug cap` enters per-axis Capability.
- `debug gen` enters the gravity-generator inspector.
- `debug mass` enters the artificial-mass inspector.
- `debug perf` enters Performance.
- `debug next` and `debug prev` navigate only within the active section.
- `debug off` returns to the Console Status standby page.

## Pages

The standby Status page shows Status, Overall, Axes READY count, and subsystem-only Warnings in that order, with aligned labels for LCD readability. Debug sections retain the validated v0.3.5 field order while improving spacing for Discovery counts, PEM frame/contribution state, one Capability page per axis, one generator per page, one artificial-mass block per page, and performance timing.

Component inspectors use Entity ID as the canonical physical component identifier.
