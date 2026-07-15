# Changelog

## v0.3.0.1

- Fixed capability assessment readiness so each axis reports READY when it has one or more validated contributing generators.
- Fixed redundancy tolerance to report `max(0, contributing generator count - 1)` consistently for Forward, Backward, Left, Right, Up, and Down.
- Updated redundancy display text to show either READY or NOT READY, avoiding mixed READY/NO output.
- Preserved Milestone 3 PEM, discovery, orientation, projection, debug, and monitor-only architecture.

## v0.3.0

- Added automatic block orientation and gravity projection-axis assessment.
- Expanded PEM metadata with validation and contributing states.
- Added dynamic per-axis capability groups with READY, generator count, and tolerance values.
- Added overall propulsion health and PEM summary displays.
- Added generator and artificial-mass debug inspectors with stable IDs and one component per page.
- Preserved monitor-only operation from Milestone 2.5.
