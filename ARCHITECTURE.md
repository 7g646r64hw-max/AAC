# AAC Architecture

## Milestone 3 Runtime Flow

1. `HardwareDiscovery` scans same-construct hardware and tag membership only.
2. `PhysicsEngineModelBuilder` builds the PEM and caches measured and derived metadata.
3. `CapabilityAnalysis` reads only the PEM to calculate health and READY axes.
4. `DisplayManager` and `DebugManager` consume cached PEM data without mutating hardware.

## PEM Authority

The PEM is the single source of truth. Measured values are entity ID, enabled state, working state, world position, and block orientation. Derived values are mount position, gravity projection axis, validation state, contributing state, dynamic capability groups, redundancy, tolerance, and propulsion health.

## Monitor-only Boundary

The controller is explicitly locked to display and Echo output. No control outputs are written in Milestone 3.
