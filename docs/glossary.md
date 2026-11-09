# Glossary

| Term | Meaning in Fish |
| --- | --- |
| Fish | Working project title; also the individual creature controlled by the player. |
| Hatchling | The vulnerable starting state of a new fish's life. Exact size and species remain open. |
| Run / life | One fish's existence from hatching to terminal death; may span many sessions. |
| Session | One period of play within a run. Leaving a session does not end a life. |
| Active run | The living fish and world state eligible for continuation. |
| Run ID | Stable identity used to associate one fish, its events, save state, death, and completed-life entry. |
| Growth | Persistent change in the same fish's size and relevant capabilities through the adopted feeding rules. |
| Edible target | Food or prey that satisfies the actual feeding rule; smaller appearance alone is not an implementation contract. |
| Cover | Environmental space that can help a fish evade a threat, subject to its size and real collision rules. |
| Burst | Proposed short speed action used in pursuit or escape; exact costs and bindings are undecided. |
| Terminal death | An authoritative gameplay outcome that permanently ends that run. |
| Suspend / resume | Save the current living state and later restore it without offline simulation. |
| Life event | Recorded, meaningful gameplay fact that can support statistics or a narrative claim. |
| Recounting / life story | Brief event-backed narrative presented after death and stored for later viewing. |
| Life History | Main-menu collection of completed fish lives and their saved stories. |
| Archive entry | One durable completed-life record keyed by run ID, independent of the active save. |
| First person | Optional camera observing the same fish and simulation from an eye-level view. |
| Godot Editor MCP | Selected editor integration used for supported inspection and automation; exact setup belongs in development documentation. |
| Phase | A bounded stage in the [roadmap](roadmap.md). |
| Gate | Observable acceptance criteria and evidence required to qualify a Phase outcome. |
| GREEN | Required checks have passed with appropriate evidence; a plan, build, or owner acceptance of a gap is not by itself a passed check. |
| Bootstrap | Initial Godot/development setup; distinct from the later playable survival loop. |

[The decision register](decisions-and-questions.md) owns the status of unresolved terms and mechanics. [The workflow](workflow.md) owns planning and qualification terminology.
