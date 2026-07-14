# BUILD_NOTES.md

## Repository Convention

- `src/AAC.cs` is the authoritative Programmable Block script.
- This file must compile by direct copy/paste into Space Engineers.
- No outer `Program` class.
- No `using` directives.
- Nested helper classes remain inside the PB-generated Program class.

This convention prevents duplicate Program class compilation errors.
