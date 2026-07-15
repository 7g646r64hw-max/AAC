# AAC Design Document

## Milestone 3 Design

Milestone 3 keeps the Milestone 2.5 architecture and extends it through the PEM. Discovery remains simple and non-authoritative: it only discovers blocks and counts LCD routes, alarms, and lights. The PEM converts discovered blocks into cached component records containing measured metadata and derived assessment data.

Per-axis capability is dynamic. A ship axis is READY when at least one contributing gravity generator and one contributing artificial mass block project on that axis. Tolerance is calculated as the spare matched pair count for the axis.

Debug pages are read-only. Summary pages remain available, while generator and artificial-mass inspectors show one component per page and use `debug next` / `debug prev` within the active inspector.
