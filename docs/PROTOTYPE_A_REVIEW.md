# Prototype A — first playable review

Status: first six encounters approved as the design baseline on 24 October 2026. Release remains on hold until explicit user approval.

## What to review

Six short encounters replace the long opening for this prototype:

1. **First launch:** two broad circle-to-circle jumps.
2. **Across the screen:** three diagonal jumps with a smooth lobed landing.
3. **A worthwhile choice:** a broad safe route and a smaller optional three-dot route, reconnecting at one exit.
4. **Lead the landing:** one slow moving support between broad recovery landings.
5. **Rain crossing:** six supports, a small branching graph, and three reward dots total.
6. **First black hole:** a readable vortex beside a broad safe route.

Movement speed, the smaller collider, contact-based dot collection and the minimal visual direction stay consistent. Encounter entry gives twelve seconds before lava starts its eased rise; first encounters do not use the old idle-punishment acceleration.

## Progression

- Completing an encounter unlocks the next immediately.
- Active play across attempts opens one new encounter every thirty seconds. All six are available by 150 active seconds.
- Three failed attempts also offer the next encounter, without marking the current one complete.
- A miss restarts the current short encounter with fresh motion and hazard state.
- Journey shows introduced encounters and records completions separately from availability.
- A successful next-encounter transition keeps uninterrupted run distance. Retrying or selecting an encounter resets it; selecting a later encounter awards no skipped distance.
- Local discovery state has its own save key. Legacy high scores are preserved, and prototype scores are not submitted to the leaderboard.

## Review procedure

Start with First launch. Judge the first six encounters before asking for more content. In particular:

- Is the next target clear before committing?
- Does the diagonal passage feel different from the first one?
- Does the fork present a real choice?
- Is the moving target readable and forgiving?
- Is rain readable with just three rewards?
- Can the vortex be recognised before entering its capture area?
- Does the short retry loop encourage another attempt?

Do not change the collider or jump speed during this first layout comparison. Record specific confusing landings and repeated failures rather than trying to fix every variable at once.

## Validation

The generation/build command is `Mektra.Editor.DiscoveryPackBuilder.Build`. It validates actual launches across the supported characters, later movement phases, encounter completion, explicit reward budgets, bounded instance spawning, discovery availability, save serialization and real vortex capture.

The generated encounter data and validation summary live in `assets/levels/discovery/`. A build passing these checks is ready for human review; it does not establish that the game is enjoyable or approved for release.

The player-free viewer uses the same prototype pack. Launch with `-mektraLevelTour` to browse all six encounters without affecting progress. Reopen normally to play.

## Feedback log

| Encounter | Observation | Proposed small revision | Result |
| --- | --- | --- | --- |
| All six encounters | User feedback: “i felt all six are awesome.” | Preserve these encounters and their movement, collider, reward reach and retry rules as the comparison baseline. | Accepted for expansion into Prototype B; this is not release approval. |

### Automated result for this build

Passed: 24 connections across 10 characters, 20 additional motion-phase samples, explicit reward budgets, completion, bounded spawning, discovery availability, save serialization and real vortex capture. Every main-route shared launch window is at least 0.30 seconds; the smaller optional bonus route has a 0.233-second minimum. The plan’s 0.35-second discovery target remains a playtest tuning goal, rather than a claim that every route already meets it.
