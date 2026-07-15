# AAC Release Notes v0.3.5

Milestone 3.5 is an Engineering Console refinement release. It preserves the verified v0.3.0.1 Hardware Discovery, Physics Engine Model, Capability Assessment, engineering calculations, and monitor-only behavior.

## Highlights

- Engineering Console standby page now uses the specified `AAC CONSOLE` / `Status` header and field order.
- Debug pages now use `AAC CONSOLE  DEBUG` headers and section-specific navigation.
- Capability is shown as one page per ship axis.
- Generator and artificial-mass inspectors show one physical component per page and use Entity ID as the canonical identifier.
- Warnings summarize subsystem categories instead of naming individual components.
- Performance reports last scan, average scan, PEM build time, and capability assessment time.

## Compatibility

`src/AAC.cs` is a single self-contained Space Engineers Programmable Block script. No support files, partial classes, or external C# sources are required.
