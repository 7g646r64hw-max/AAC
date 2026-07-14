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

## Milestone 1 Foundation Behavior Preserved

AAC remains intentionally observational:

1. Discover relevant blocks on the same construct as the programmable block.
2. Summarize whether a ship controller and at least one gravity-generator / artificial-mass pair are present, and report explicit findings for missing required hardware.
3. Publish concise status to flight LCDs and detailed POST data to maintenance and engineering LCDs.
4. Keep propulsion and alert outputs disabled until calibration and solver behavior are implemented.

## Milestone 2 PEM Behavior

Milestone 2 adds the first read-only PEM pipeline:

1. `HardwareDiscovery` scans the same construct.
2. `HardwareSnapshot` stores detected hardware counts, selected controller data, and detailed metadata for AAC-owned propulsion blocks.
3. `PhysicsEngineModel` is built from AAC-owned propulsion hardware only.
4. `CapabilityAnalysis` evaluates whether the PEM has a valid coordinate frame and tagged gravity-generator / artificial-mass pair.
5. `DisplayManager` reports model and capability status without issuing outputs.

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
- **Engineering**: runtime state, PEM readiness, tagged generators, tagged artificial mass, coordinate validity, capability analysis status, `Control Output: LOCKED`, and discovery snapshot.
