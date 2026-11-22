# Prior art: what Paradox's engineers say about simulation performance

**Status:** research note, 2026-11-21. Written by Sophie (Planner) for Clint to decide on. It changes nothing in `ENGINE_SPEC.md` by itself.

**Question:** ENGINE_SPEC §12 says Paradox-style engines choke for four reasons, and that staggered cadence, commitment and pure `Decide` fix all four. Does what Paradox's own engineers have published support that?

**Method:** public material only: dev diaries by tech leads and programmers, plus one conference talk. Nothing decompiled or reverse-engineered. Their code is closed and stays unread, so this design stays our own.

## Short answer

The diagnosis holds on all four counts. In most places Paradox has been **retrofitting the architecture this spec starts with**. One correction to wording, and two tensions worth writing down before they bite.

## The four causes, checked

| Spec's cause | Verdict | What the sources show |
| --- | --- | --- |
| Everything ticks daily | **Partly right, worded too strongly** | They do have cadence tiers (Victoria 3: yearly, monthly, weekly, daily and sub-daily tick tasks). But a weekly tick also runs all the daily and sub-daily work, so the heavy tiers stack on one tick. Cadence gets added one feature at a time: CK3 moved AI council progress from daily to monthly, cutting that cost to 1/30. |
| AI reassesses from scratch | **Confirmed** | CK2 added a per-decision `ai_check_interval`. Raid armies were changed to re-evaluate only with a compelling reason or after a while. That's commitment plus an interrupt set, arrived at as a performance patch. CK3 added a war coordinator to cut redundant per-AI decisions. |
| Granularity inflates | **Strongly confirmed** | Victoria 3: employment cost scales with the **number of pop objects**, not total population. Late-game tiny pops made it slow, so 1.2 merges small pops. Stellaris 4.0 grouped pops (×100 scale) so production is checked once per planet instead of once per pop, explicitly for late-game performance. |
| Entanglement blocks safe parallelism | **Strongly confirmed** | HOI4's tech lead: units and combat can't be multithreaded because movement runs in sequence, one nation before the next, a board-game legacy. That's exactly the turn-order mutation §3.2 forbids. |

## Where the spec is already ahead

The parallelism pattern Paradox is moving toward is the spec's tick phases, retrofitted:

- **CK3:** each system update is split into a part that only touches the entity's own data and a part that can write shared state. Changes to visible state are stored and applied serially afterwards. The AI never changes gamestate directly. In this spec, that's Decide (pure) followed by Resolve (the only mutation).
- **Their proposed next step** (ACCU 2023 talk): entities that read only other entities' *public* data, with no ordering requirement. In this spec, that's the frozen snapshot plus `WhatASeesOfB`.
- **Victoria 3** needs an extra sort step after parallel work to keep multiplayer in sync. Read-old/write-new with a deterministic Resolve order makes that the default instead of a patch.

## Correction

**The engine isn't the culprit.** Paradox's tech lead says Clausewitz itself mostly handles graphics, networking and loading/saving, and that the bulk of the bottleneck is in each game's own simulation. §12 should say "Paradox-style *simulations*", not "engines". Wording only; the reasoning doesn't change.

## Two tensions to record (not decide)

These don't conflict with §9's "do not optimize". They're notes so that future-us recognizes the problem when the profile shows up.

1. **§3.6 "never maintained incrementally" vs. Victoria 3's biggest cost.** Modifiers were Victoria 3's main cost driver. They fixed it by tracking which values were dirty and recomputing only those, which is incremental maintenance. At 32 cells, full recompute is right. At global scale, derived-value recompute is where the spec's own rule will be tested first.
2. **Resolve becomes the serial bottleneck, and big nations become the long pole.** Victoria 3's employment has a serial core because it creates and destroys objects; HOI4's serial parts are combat and movement. Under this spec all mutation is in Resolve, so Resolve is where serial cost collects. Separately, Paradox's work per country varies by more than 100× (Luxembourg vs. Russia), and a parallel system only finishes when its slowest entity does. §9 is right that nation *count* is nearly free. Variance in per-nation cost isn't, once Decide runs in parallel. This touches the open simultaneous-conflict rule in §11, so it stays a note.

## One cheap guard worth considering

Granularity inflation is the most consistent failure across their games: object counts that grow with time. A test, or a line in the Chronicle, that reports entity counts (nations, cells, in-flight actions) per N ticks would catch it the day it starts. Your call whether that becomes a ticket.

## Your own data source

HOI4 has a built-in profiler: run `imgui show profiler` from the console. It shows the cost of the last tick and the average over the last 24 ticks. Your Millennium Dawn soak-test harness could log those numbers across a long campaign. That's black-box evidence from a real Paradox game of which cadences spike, with no source code involved.

## Not read

The Imperator "Anatomy of a Game: Simulation Optimization" post, CK2 dev diary #71, and EU5's Tinto Talks #123 (performance) were behind browser checks this session. Only the CK2 #71 search excerpt is used above. None of them publish AI cost numbers as far as the excerpts show.

## Sources

- [Victoria 3 Dev Diary #76: Performance](https://www.paradoxinteractive.com/games/victoria-3/news/dev-diary-76-performance)
- [CK3 Dev Diary #36: Gotta Go Fast](https://admin-forum.paradoxplaza.com/forum/developer-diary/ck3-dev-diary-36-gotta-go-fast.1408620/page-8)
- [CK2 Dev Diary #71: AI and Optimization](https://forum.paradoxplaza.com/forum/threads/ck2-dev-diary-71-ai-and-optimization.1050551/page-2) (search excerpt only)
- [HOI4 Dev Diary: A Tech Lead's Life](https://admin-forum.paradoxplaza.com/forum/developer-diary/hoi4-dev-diary-a-tech-leads-life.1496472/page-5)
- [HOI4 Developer Diary: Performance (1.13 changelog)](https://admin-forum.paradoxplaza.com/forum/developer-diary/developer-diary-performance-changelog.1600827)
- [Mathieu Ropert, "Multithreading Model in Paradox Games: Past, Present and Future", ACCU 2023](https://accu.org/conf-docs/PDFs_2023/XMultiThreadingModelinParadoxGamesPastPresentandFuture.pdf)
- [Stellaris Dev Diary #370: 4.0 Changes Part 4](https://admin-forum.paradoxplaza.com/forum/developer-diary/stellaris-dev-diary-370-4-0-changes-part-4.1728047)
