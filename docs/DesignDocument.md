# Adaptive Antigravity Controller

## Mission

Observe. Understand. Adapt.

## Personas

- Pilot
- Maintenance
- Designer
- Developer

## Philosophy

The AAC discovers arbitrary gravity-drive configurations and builds an internal
Physics Engine Model (PEM). The controller never assumes a fixed ship layout.

The informal term 'PEM Drive' refers only to the physical gravity-generator and
artificial-mass propulsion system and is not used within the software UI.

## Milestone 1 Foundation Behavior

The first implementation increment is intentionally observational:

1. Discover relevant blocks on the same construct as the programmable block.
2. Summarize whether a ship controller and at least one gravity-generator /
   artificial-mass pair are present, and report explicit findings for missing
   required hardware.
3. Publish concise status to flight LCDs and detailed POST data to maintenance
   and engineering LCDs.
4. Keep propulsion and alert outputs disabled until calibration and solver
   behavior are implemented.

## Display Personas

- **Flight**: concise version, operating mode, status, selected controller name,
  gravity generator count, artificial mass count, and tick count.
- **Maintenance**: POST summary, discovered hardware counts, display counts, and
  recent tick-labeled events.
- **Engineering**: architecture state and placeholders for PEM, capability
  analysis, and solver milestones.
