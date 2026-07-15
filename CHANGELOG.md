# Changelog

## v0.3.0-alpha.1

- Promoted the Physics Engine Model to the authoritative digital twin for AAC-owned propulsion hardware.
- Added derived block orientation and gravity projection axis metadata from game orientation matrices.
- Expanded artificial mass and generator metadata with enabled, working, validated, contributing, distance, mount position, and stable debug IDs.
- Added generator lifecycle tracking: discovered, owned, working, validated, contributing.
- Added dynamic per-axis capability groups with READY status, generator count, tolerance count, and not-ready reasons.
- Updated Engineering PEM Summary with health, reference controller, coordinate frame, generator/mass counts, and redundancy.
- Replaced list-style component debug pages with dedicated `debug generators` and `debug mass` inspectors.
- Preserved strict monitor-only operation and `[AAC]` ownership rules.

## v0.2.5-alpha.1

- Bumped AAC release references from `v0.2.0-alpha.1` to `v0.2.5-alpha.1`.
- Added the read-only DebugManager validation framework.
- Added debug page navigation and programmable-block Echo status.
- Preserved monitor-only Milestone 2 behavior.
