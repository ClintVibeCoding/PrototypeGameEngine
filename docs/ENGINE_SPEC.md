# Grand Strategy Simulation — Engine Specification

**Status:** Engine specified. Game design deliberately unsettled.
**Language:** C#
**Read this before writing code. Do not invent answers to anything in "Open Design Questions."**

---

## 1. What This Is

A deterministic simulation engine for a grand strategy world. There is **no player**. The output is a text chronicle of what nations did and why, with map rendering as a later stretch goal.

The purpose of v1 is to prove the *architecture*, not the game. Actions can be placeholders. The engine must be correct before the design exists.

### Milestone / Definition of Done

Three things must be true:

1. **Determinism.** Same seed → byte-identical chronicle, across runs and across machines.
2. **Divergence.** Two different disposition sets on the same map produce visibly different histories.
3. **Legibility.** The chronicle is readable by a human who wasn't watching.

If all three hold with placeholder actions, the engine is done and design work can begin.

---

## 2. Scope

**In scope for v1**

- 8 nations
- Ireland as the map: ~32 counties as cells, 4 per nation at start
- Perfect information
- Text chronicle output

**Explicitly deferred — do not build, but do not architecturally preclude**

- Units moving on the map (v2+)
- Fog of war
- A player
- Map rendering
- Near-future layer (physical and cyber infrastructure tiers)

---

## 3. Engine Invariants

These are non-negotiable. Everything else is open.

### 3.1 Determinism is the product

- One explicitly seeded RNG, **passed through** to anything that needs it. No global/ambient random. No `DateTime.Now`. No thread IDs. No hash-order dependence.
- Use ordered collections everywhere iteration order can affect outcomes. `Dictionary` iteration order is not a contract — do not rely on it.
- Prefer integer or fixed-point arithmetic for anything that feeds a decision. Floats are permitted only where a difference in the last bit cannot change a branch.
- Determinism is tested, not assumed. See §8.

### 3.2 Read old, write new, swap

Within a tick, every actor observes the **same frozen snapshot**. Nothing any nation does mid-tick is visible to any other nation until the tick ends. All mutations land together at the end.

Rationale: without this, turn order becomes a hidden advantage. The sim would still be deterministic — just wrong in a way that only shows up as "nation 1 always does well."

### 3.3 Decide is a pure function

`Decide(worldSnapshot, nation) -> intentions`

It mutates nothing. It returns intentions. `Resolve` does all mutation.

Consequences worth protecting:
- Any tick's decisions can be replayed against a saved state and reproduce identical intentions. This is the primary debugging tool.
- Decision logic is decoupled from state *storage*, so storage layout can change later for performance without touching decisions.
- Pure decide parallelizes trivially and deterministically. Do not parallelize yet — just don't break the property.

### 3.4 One clock

There is a single tick loop. Different systems run at different cadences *within* it. There must never be two loops.

### 3.5 Three clocks, separated

| | What it is | Rule |
|---|---|---|
| **Engine tick** | Integer counter | What determinism is defined against |
| **Calendar** | Pure function of tick → in-world date | Display only. **No logic may ever read it.** |
| **Wall clock** | How fast ticks run for a watcher | Presentation only. Must have zero effect on outcomes. |

Calendar-shaped events (budgets, cycles) are expressed in ticks — "every 12 ticks" — and the calendar function makes them *look* annual. The sim never asks "is it January."

### 3.6 Derived values are derived

Borders, neighbor sets, infrastructure totals, deltas — all recomputed at end of tick from authoritative state. Never maintained incrementally. Cheaper to reason about, impossible to desync.

---

## 4. Tick Phases

Every tick runs all five, in order:

1. **Observe** — freeze the snapshot
2. **Decide** — pure; produces intentions
3. **Resolve** — apply intentions and mature in-flight actions; the only mutating phase
4. **Derive** — recompute derived values
5. **Log** — emit chronicle entries

Each phase is a separate, individually loggable, individually steppable function. Do **not** implement alternating tick types — parity would make the tick counter non-uniform and complicate every cadence rule downstream.

---

## 5. Object Shapes

### Nation

- `People`
- `Money`
- `Power` (land only)
- `Cohesion`
- `CurrentActivity` — what it's in the middle of, and ticks remaining

That's the whole object. Keep it small. Complexity should come from interaction rules and the map, not fat objects.

Decisions read both **stocks and trends**. A nation at 100 money and rising is in a different position than 100 and falling, and a utility function reading only the stock cannot tell them apart. Deltas are derived from last tick's values — do not store them as fields.

### Cell

- Owner
- Infrastructure
- Adjacency (land and sea edges, with differing costs)
- Coordinates (for eventual rendering)

Infrastructure is territorial, not national — so it can be lost by losing ground.

Sea edges cheap, land edges expensive. Sea/land power asymmetry is expected to **emerge** from edge costs. Do not code geopolitical theory explicitly.

### In-Flight Action

- Actor
- Target
- Completion tick
- Enough state to be inspected

These are **running, not queued.** They are observable and interferable mid-flight. This is not a stack and must not be LIFO — resolution order is by scheduled completion tick, not by order of entry.

**Revalidate at resolution.** An action can become nonsense before it lands (target already taken, actor no longer has the committed power). Without this check you get ghost invasions of nations that already collapsed.

---

## 6. Mechanics the Engine Must Support

### Commitment

Nations lock an action for N ticks and do not reassess. This is not primarily an optimization — it prevents **oscillation**, where a nation flips between two closely-scored options every tick and the chronicle reads as a lunatic. Commitment is what turns a scoring function into something that looks like policy.

A small **interrupt set** forces early reassessment regardless of the timer (attacked; target gone; threshold collapsed). Without it you get sunk-cost absurdity.

Commitment length and interrupt sensitivity are per-nation disposition traits.

### Policies vs. Acts

- **Policy** — a standing dial that applies every tick until changed. Primary example: allocation, splitting income across cohesion / infrastructure / power.
- **Act** — happens once, resolves, is over.

Most ticks, nothing is decided at all. Policies just apply.

### Irreversibility

Whether an action can be recalled once committed is a **designed property of that action**, not a byproduct of its duration. Some multi-tick actions should be uncancellable. This is what makes disposition matter.

### Decision method

Utility scoring. Weighted considerations, pick the top-scoring action. **No search trees** — the branching factor is the thing that kills AI in this genre.

Every nation shares the **same utility function**. All differentiation comes from position (endowment) and disposition (weights). Nations do not have assigned goals; goals emerge from circumstance.

---

## 7. Seams — Cheap Now, Expensive Later

Build these even though they currently do nothing.

| Seam | Now | Later |
|---|---|---|
| `WhatASeesOfB(a, b)` | Returns B's true state | Single change point for fog of war |
| `TransferTerritory(cell, newOwner)` | Direct assignment | Units call this instead |
| Calendar | Function of tick | Stays display-only forever |

Every decision that reads another nation's state must go through `WhatASeesOfB`. If decisions reach into other nations directly, adding fog later means touching every decision in the sim.

Do not build a snapshot-and-store intelligence phase. Under perfect information there is nothing to update — intelligence is a lookup at the moment of decision, not a tick phase. A contact phase appears only when beliefs do.

---

## 8. Testing

1. **Determinism test.** Run seed X twice. Assert identical chronicle. This runs on every change.
2. **Divergence test.** Same map and seed, two disposition sets. Assert the chronicles differ meaningfully.
3. **Replay test.** Save a world state, replay `Decide` against it, assert identical intentions.

Test 1 is the one that must never go red.

---

## 9. Performance Posture

**Do not optimize.** At 8 nations and 32 cells, every performance instinct is unverifiable. There is no profile to point at.

Scalability work right now is **structural only**: hold the seams, keep `Decide` pure, one loop, nothing reads the calendar.

When scale does matter, the cost is **not** nation count — that is nearly free under this architecture. The superlinear cost is **cell count and per-cell systems that touch neighbors.** Design against that, not against nation count.

---

## 10. Placeholder Actions for v1

The engine milestone does not require the real action set. Implement the *thinnest* actions that exercise the machinery:

- One instant act
- One multi-tick, cancellable act
- One multi-tick, irreversible act
- One standing policy (allocation)

They do not need to be interesting. They need to prove durations, commitment, interrupts, revalidation, and simultaneous resolution all work.

---

## 11. Open Design Questions — DO NOT INVENT ANSWERS

These are unsettled by choice. If the implementation needs one, **stub it and flag it**, don't decide it.

- **Action set.** Direction: build / allocate / spend power, where "spend power" splits into attack / coerce / suppress as distinct verbs. Coercion that does not result in fighting is where the interesting histories live.
- **Tick length.** Leaning one month at nation scale. Not final.
- **Simultaneous-conflict resolution rule.** Must exist and must be deterministic. Crude is acceptable. Durations make collisions rare but do not remove the need.
- **Cohesion mechanics.** What raises it, what erodes it, what happens at zero.
- **Disposition weight set.**
- **Momentum / inertia** beyond commitment.
- **Run length in ticks.**

---

## 12. Design Notes Worth Keeping

- The two genre problems — "the AI can't use the features you give the player" and "too many little processes that scale with time" — are the **same problem**: mechanics that are expensive to evaluate. The design constraint is therefore *can a mechanic be made cheap to evaluate without being shallow to play?*
- Paradox-style engines choke because everything ticks daily, AI reassesses from scratch, granularity inflates, and entanglement blocks safe parallelism. Staggered cadence, commitment, and pure `Decide` address all four.
- Map painting requires cells with coordinates rather than an abstract graph. The chronicle still matters: the map shows *that* something happened, the log shows *why*. Colour spreading without reasons gets old fast.
- Identical nations on an asymmetric map is a useful **diagnostic**: if they still diverge, the interaction rules are doing real work.
