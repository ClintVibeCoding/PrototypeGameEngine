# Process v0

**Status:** v0, written before the first run. It is expected to be wrong in places. `retro` corrects it after each real run; don't polish it ahead of evidence.

## Who does what

| Role | Who | Owns |
| --- | --- | --- |
| Director | Clint | Decides what to build, answers grilling, approves ticket breakdowns, merges one-way doors |
| Planner | Sophie (Claude, chat) | Grilling, specs, tickets, reviewing findings, updating this doc |
| Builder | Claude Code (DadPC) | Implements tickets test-first, keeps CI green, merges two-way doors |
| Finder | Local model (Qwen 9B via LM Studio / Bionic) | Bulk reading: narrow review passes, log digests, exploration notes. **Finds, never approves.** |
| Gate | CI (GitHub Actions) | Lint, typecheck, tests. Deterministic, costs no tokens, decides merge eligibility |

## The loop

1. **Grill:** `/grill-with-docs` on the feature. Output: decisions agreed, `GLOSSARY.md` updated, ADRs only if hard to reverse, surprising, *and* a real trade-off.
2. **Spec:** `/to-spec` turns the grilling into a spec (GitHub issue).
3. **Tickets:** `/to-tickets` turns the spec into tracer-bullet tickets (sub-issues). Each ticket is verifiable on its own and fits one fresh session. Clint approves the breakdown before anything is built.
4. **Build:** Claude Code takes one ticket at a time, test-first (`tdd`), on its own branch. One ticket = one session = one PR.
5. **Gate:** CI must be green. No green, no review.
6. **Find:** the local model runs narrow review passes on the diff (one file, one question at a time) and writes findings to a file attached to the PR.
7. **Judge:** Claude reads the findings, not the whole diff: confirms or dismisses each one, fixes the real ones.
8. **Merge:** per merge danger (below).
9. **Retro:** `/retro` after each ticket. A mechanical mistake becomes a CI check; a judgement call goes into `CODING_STANDARDS.md`; process fixes go into this file.

Then the next ticket. Never start ticket N+1 before ticket N is merged and verified.

## Merge danger

Every PR states two things:

- **Door:** two-way (easy to undo) or one-way (data loss, irreversible, external effect).
- **Blast radius:** small (one module) or big (cross-cutting).

Two-way and small: Claude Code may merge once the gate is green. Anything else: Clint reviews and merges.

## Rules

- **Determinism beats a model.** If a check can be a script, it's a script, not a prompt.
- **Small bites.** Each piece verified before the next starts.
- **Shared language.** Code, tickets and chat use the terms in `GLOSSARY.md`. A conflict gets called out, not worked around.
- **Branches only.** Nobody commits to `main` directly; everything lands by PR.
- **Push before handoff.** Whoever stops working pushes before the other one starts.
- **The local model never approves.** Its output is a list of leads for Claude to check.

## Where things live

| Thing | Location |
| --- | --- |
| This process | `docs/PROCESS.md` |
| Domain terms | `GLOSSARY.md` (created when the first term is settled) |
| Decisions | `docs/adr/NNNN-slug.md` (created when the first ADR is needed) |
| Review standards | `CODING_STANDARDS.md` (created by the first retro that needs it) |
| Specs and tickets | GitHub issues, tickets as sub-issues of their spec |
| Skill config | `docs/agents/` (written by `/setup-matt-pocock-skills`) |

## Open questions for the first run

- Can the finder step run unattended? Bionic documents no CLI or scheduling. Fallback: a script calling LM Studio's local server.
- Does Matt's `code-review` skill load into Bionic as-is?
- First calibration: plant known bugs on a branch, run the finder, count what it catches. That number decides how much the loop trusts step 6.
