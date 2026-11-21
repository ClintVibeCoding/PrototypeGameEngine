# Grand Strategy Simulation

A deterministic simulation of nations with no player, which produces a written history of what each nation did and why.

## Time

**Tick**:
One complete pass through the five Phases, identified by its tick number. Tick 0 is the starting World before any Tick has run; the first Tick to run is tick 1.
_Avoid_: turn, step, round, frame, update

**Phase**:
One of the five fixed steps of a Tick, always run in this order: Observe, Decide, Resolve, Derive, Log.
_Avoid_: stage, step, pass

## State

**World**:
The complete state of the simulation at the end of a Tick, including the values derived from it. A World never changes once it exists; each Tick produces a new one from the previous.
_Avoid_: state, game state, board

**Snapshot**:
The World that every Nation observes during a Tick: the World from the end of the previous Tick. Every Nation sees the same Snapshot.
_Avoid_: frame, view, copy

## Nations and territory

**Nation**:
One of the independent powers whose choices make up the simulation. Every Nation is decided for by the same rules; differences come only from its position and disposition.
_Avoid_: country, faction, player, actor

**Cell**:
One unit of territory on the map, owned by exactly one Nation.
_Avoid_: tile, province, county, region

**Holdings**:
The number of Cells a Nation owns, worked out fresh from the World each Tick rather than kept as a running count.
_Avoid_: territory count, size

**Intention**:
What a Nation means to do this Tick, formed by observing the Snapshot. Intentions take effect only when the Tick resolves.
_Avoid_: action, order, command, move

## Runs

**Seed**:
The single number that, with the starting inputs, fully determines a run.
_Avoid_: random seed, key

**Chronicle**:
The ordered, human-readable record of a run: what happened, tick by tick. Two runs from the same Seed produce identical Chronicles.
_Avoid_: log, history, output, report

**Chronicle entry**:
One item in the Chronicle, belonging to a single Tick.
_Avoid_: log line, event, message
