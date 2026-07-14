# AAC Architecture

Core philosophy:

- The ship designer engineers the hardware.
- The AAC understands the hardware.
- The pilot receives concise operational information.
- The system adapts rather than requiring a prescribed layout.

## Runtime Subsystems

- **AAC Core**: owns the update loop and coordinates discovery, diagnostics, and
  display rendering.
- **HardwareDiscovery**: scans the same construct as the programmable block and
  builds a hardware snapshot without assuming a fixed ship layout.
- **Configuration**: centralizes AAC tags used for optional operator blocks.
- **Diagnostics**: performs the current POST readiness check.
- **DisplayManager**: renders separate flight, maintenance, and engineering LCD
  views when tagged panels are present.
- **EventLogger**: stores a bounded in-memory event history for maintenance LCDs.

## Planned Subsystems

- CalibrationEngine
- PhysicsEngineModel (PEM)
- CapabilityAnalysis
- GravitySolver
- GeneratorController
- AlertManager

## Safety Posture

`v0.1.1-alpha.1` is monitor-only. It discovers and reports hardware but does not
modify gravity generator fields, artificial mass state, alarms, warning lights, or
ship motion.
