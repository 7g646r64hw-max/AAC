# Adaptive Antigravity Controller

## Mission

Observe. Understand. Adapt.

## Personas

- Pilot
- Maintenance
- Designer
- Developer

## Philosophy

The AAC discovers arbitrary gravity-drive configurations and builds an internal Physics Engine Model (PEM). The PEM is the authoritative internal representation of the gravity-drive system. Future control logic, calibration, and solver functionality must consume the PEM rather than querying hardware directly.

The informal term 'PEM Drive' refers only to the physical gravity-generator and artificial-mass propulsion system and is not used within the software UI.

No subsystem may influence ship behavior until it can fully explain its own state through DebugManager.

## Milestone 1 Foundation Behavior Preserved

AAC remains intentionally observational:

1. Discover relevant blocks on the same construct as the programmable block.
2. Summarize whether a ship controller and at least one gravity-generator / artificial-mass pair are present, and report explicit findings for missing required hardware.
3. Publish concise status to flight LCDs and detailed POST data to maintenance and engineering LCDs.
4. Keep propulsion and alert outputs disabled until calibration and solver behavior are implemented.

## Milestone 2 PEM Behavior

Milestone 2 added the first read-only PEM pipeline:

1. `HardwareDiscovery` scans the same construct.
2. `HardwareSnapshot` stores detected hardware counts, selected controller data, and detailed metadata for AAC-owned propulsion blocks.
3. `PhysicsEngineModel` is built from AAC-owned propulsion hardware only.
4. `CapabilityAnalysis` evaluates whether the PEM has a valid coordinate frame and tagged gravity-generator / artificial-mass pair.
5. `DisplayManager` reports model and capability status without issuing outputs.

## Milestone 2.5 Debug Behavior

Milestone 2.5 adds a permanent read-only DebugManager:

1. DebugManager handles debug commands and debug page selection.
2. DebugManager reads current subsystem snapshots through DisplayManager rendering paths and never mutates subsystem state.
3. Engineering LCDs become the debug display while debug mode is enabled.
4. Flight and Maintenance LCD behavior remains unchanged.
5. Programmable-block `Echo()` always includes a debug status line.
6. Debug failures must not stop discovery, diagnostics, PEM updates, or capability analysis.

Supported debug commands:

- `debug on`
- `debug off`
- `debug pem`
- `debug discovery`
- `debug capability`
- `debug performance`
- `debug next`
- `debug prev`

Debug pages:

- Overview
- Discovery
- PEM Summary
- Generator Inspector
- Artificial Mass Inspector
- Capability Analysis
- Performance Placeholder

## Hardware Ownership Rule

Only gravity generators and artificial mass blocks with `[AAC]` in their Custom Name are AAC-owned propulsion hardware. Only those tagged propulsion blocks are modeled by the PEM or eligible for future control logic. Ship controllers remain exempt so AAC can always determine the ship reference frame.

## Coordinate System

The selected ship controller defines the ship-relative reference frame:

- Forward
- Backward
- Left
- Right
- Up
- Down

Tagged propulsion block metadata includes the dominant ship-relative direction from the controller to the block.

## Display Personas

- **Flight**: concise version, operating mode, status, selected controller name, detected gravity generator count, detected artificial mass count, and tick count. This remains intentionally unchanged in philosophy.
- **Maintenance**: POST summary, discovered hardware counts, AAC-owned tagged propulsion counts, display counts, and recent tick-labeled events.
- **Engineering**: runtime state, PEM readiness, tagged generators, tagged artificial mass, coordinate validity, capability analysis status, `Control Output: LOCKED`, and discovery snapshot when debug mode is off. When debug mode is on, the Engineering LCD renders DebugManager pages.


## Milestone 3 PEM Digital Twin

AAC v0.3.0-alpha.1 makes the Physics Engine Model the single source of truth for AAC-owned propulsion hardware. HardwareDiscovery captures measured data from the game, while the PEM caches derived data such as mount position, block orientation, gravity projection axis, validation state, contribution state, deterministic debug identifiers, and lifecycle status. CapabilityAnalysis and DisplayManager consume this cached PEM state instead of querying propulsion blocks directly.

Mount position is based on the block's location relative to the selected ship controller. Block orientation and gravity projection axis are based on the block's world matrix relative to the same controller frame. AAC never derives orientation or capability from block names; names only provide the explicit `[AAC]` ownership tag.

The generator lifecycle is Discovered -> Owned (`[AAC]`) -> Working -> Validated -> Contributing. Validation requires known mount position, known block orientation, known gravity projection axis, and working state. Contribution additionally requires the component to be enabled. Artificial mass blocks use the same ownership, validation, and contribution concepts with mass-specific metadata.

Capability groups are built dynamically from contributing PEM generators per translation axis. Each axis reports READY/NO, generator count, tolerance count, and a reason such as `No contributing generators`, `Generators not validated`, `No contributing artificial mass`, or `Invalid coordinate frame`.

DebugManager remains read-only. Summary pages continue to exist, while `debug generators` and `debug mass` enter one-component-per-page inspectors. `debug next` and `debug prev` navigate within the active inspector.
