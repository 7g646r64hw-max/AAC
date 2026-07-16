# AAC Release Notes v0.3.5.1

Milestone 3.5.1 is a refinement-only Engineering Console polish release. It preserves the validated v0.3.5 engineering behavior, debug navigation, monitor-only operation, Hardware Discovery, PEM, and Capability Assessment.

## Highlights

- Improved Engineering Console LCD spacing and label alignment across Status, Discovery, PEM, Capability, Inspector, and Performance pages.
- Preserved the `AAC CONSOLE` header, with `DEBUG` appended only while viewing debug sections.
- Retained the Status page field order: Status, Overall, Axes READY, and Warnings.
- Kept subsystem-based Status warnings formatted for multiple tags, such as `Hardware: (GEN) (MASS)`.
- Retained one Capability page per axis and the existing field order on all debug pages.

## Compatibility

`src/AAC.cs` remains a single self-contained Space Engineers Programmable Block script. This release does not introduce new engineering features or alter validated v0.3.5 behavior.
