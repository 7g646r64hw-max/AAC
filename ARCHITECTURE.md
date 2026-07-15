# AAC Architecture

## Milestone 3.5 Runtime Flow

1. `HardwareDiscovery` scans same-construct hardware and tag membership only.
2. `PhysicsEngineModelBuilder` builds the PEM and caches measured and derived metadata.
3. `CapabilityAnalysis` reads only the PEM to calculate health and READY axes.
4. `DebugManager` tracks Engineering Console section state and section-local page navigation.
5. `DisplayManager` renders Flight, Maintenance, and Engineering Console text without mutating hardware.

## PEM Authority

The PEM is the single source of truth. Measured values include Entity ID, enabled state, working state, world position, and block orientation. Derived values include mount position, distance from the reference controller, gravity projection axis, validation state, contributing state, dynamic capability groups, redundancy, tolerance, and propulsion health.

Entity IDs remain the canonical identifiers for physical components in Engineering Console inspectors.

## Engineering Console Hierarchy

Diagnostics are presented in the required order: Status, Discovery, PEM, Capability, Component Inspectors, and Performance. The standby console answers whether AAC requires attention. Debug sections explain the discovered hardware, PEM model, per-axis capability, component-level generator and mass state, and scan/build/assessment timing.

## Monitor-only Boundary

The controller is explicitly locked to display and Echo output. No control outputs are written in Milestone 3.5.
