# AAC Release Notes - v0.3.0-alpha.1

Release: `v0.3.0-alpha.1`

Milestone 3 enhances the Physics Engine Model into AAC's authoritative digital twin for AAC-owned propulsion hardware while retaining the verified monitor-only safety posture.

## Highlights

- PEM now stores measured and derived state for every `[AAC]` gravity generator and artificial mass block.
- Generator orientation and gravity projection axis are derived from game orientation data, never from names.
- Mount position remains a separate derived concept based on block position relative to the reference controller.
- Validation requires known mount position, block orientation, gravity projection axis, and working state.
- Contributing hardware must be validated and enabled.
- Capability analysis dynamically evaluates each translation axis for READY state, generator count, tolerance count, and not-ready reason.
- Engineering displays show PEM health, counts, coordinate frame, reference controller, and redundancy.
- DebugManager includes one-component-per-page inspectors entered through `debug generators` and `debug mass`.

## Safety

AAC remains monitor-only. No solver, calibration, PID, propulsion output, alarm output, or flight-control output is enabled in this release.
