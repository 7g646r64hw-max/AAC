# AAC Design Document

## Milestone 3.5 Design

Milestone 3.5 keeps the verified v0.3.0.1 engineering model intact and refines only the Engineering Console presentation. Discovery remains simple and non-authoritative: it discovers same-construct blocks and tag membership. The PEM converts discovered blocks into cached component records containing measured metadata and derived assessment data.

Per-axis capability remains PEM-driven. A ship axis is READY when the validated v0.3.0.1 capability calculation reports one or more contributing generators for that axis, with tolerance calculated as `max(0, contributing generator count - 1)`.

The Engineering Console follows the hierarchy Status → Discovery → PEM → Capability → Component Inspectors → Performance. The standby Status page answers whether AAC requires attention. Debug section commands enter their section immediately, and `debug next` / `debug prev` navigate only within that active section.

Component inspectors show one generator or one artificial-mass block per page. Entity ID is the canonical identifier for physical components. Warnings on the Status page are intentionally subsystem-level only; component-level details belong in inspectors.

Flight and Maintenance displays remain read-only and preserve the validated pilot and technician roles. AAC remains monitor-only and does not command gravity generators, artificial mass blocks, ship controllers, alarms, lights, or other outputs.
