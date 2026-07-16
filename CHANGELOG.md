# Changelog

## v0.3.5.1

- Polished Engineering Console LCD spacing and field alignment across Status, Discovery, PEM, Capability, Inspector, and Performance pages.
- Preserved validated v0.3.5 engineering behavior, debug navigation, monitor-only operation, Hardware Discovery, PEM, and Capability Assessment.
- Retained subsystem-based warning summaries with multi-tag formatting such as `Hardware: (GEN) (MASS)`.
- Confirmed `src/AAC.cs` remains a single self-contained Space Engineers Programmable Block script.

## v0.3.5

- Implemented the Milestone 3.5 Engineering Console refinement.
- Added canonical console commands: `debug disc`, `debug pem`, `debug cap`, `debug gen`, `debug mass`, `debug perf`, `debug next`, `debug prev`, and `debug off`.
- Replaced the prior engineering standby page with the specified AAC Console Status page and subsystem-only warning summary.
- Added one-page-per-axis Capability debug pages and Entity-ID-based generator and artificial-mass inspectors.
- Added console performance fields for last scan, average scan, PEM build time, and capability assessment time.
- Preserved verified v0.3.0.1 Hardware Discovery, PEM, Capability Assessment, engineering calculations, and monitor-only behavior.


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
