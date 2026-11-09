# Durable engineering decisions

This directory holds architectural decision records when a consequential choice needs reasoning beyond the concise [decision register](../decisions-and-questions.md). No individual ADR is required by this documentation starter.

The register remains the authority for confirmed requirements, proposals, open questions, identifiers, and status. Link an ADR from its register entry rather than create a competing list of current decisions.

## Record format

Use the next unused four-digit number in `NNNN-<short-title>.md`. Record:

- Title, date, and status: proposed, accepted, superseded, or rejected.
- The concrete problem and relevant source or design context.
- The decision, its bounded scope, and the instruction or task authorizing it.
- Rationale and alternatives that materially affected the choice.
- Consequences, preserved behavior, and conditions for revisiting it.
- Links to the register, affected documents, and implementation/validation evidence.

An accepted design is not evidence that it is implemented or qualified. Current user instructions can authorize the bounded work directly; an ADR is a record, not an additional activation gate.

## Placement and updates

Routine task choices belong in [planning](../planning/README.md) or the relevant [task stack](../tasks/README.md). Use an ADR for decisions such as save compatibility or a durable state boundary when their reasoning would otherwise be lost.

When superseding a decision, retain its original rationale, link the replacement, and update the register and current owning documents. Follow [the workflow](../workflow.md) for documentation maintenance.
