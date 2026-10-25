# Prototype C — familiarity and depth

Local review build only. Release stays on hold until the user explicitly approves it.

The user approved the variations in Prototype B and authorised ten additional encounters. The first twenty are loaded from the accepted `prototype-b.json` artifact; their layouts, movement, hazards, rewards and launch rules are preserved. The new encounters use the same player size, forgiving collider, launch speed, monochrome art, blue trail and existing landing/perfect-sequence feedback.

| Encounter | Main decision | Difficulty change |
| --- | --- | --- |
| 21. The returning arc | Read a right-hand organic curve, then cross back through the centre. | A different departure direction using familiar geometry. |
| 22. Rise and settle | Lead a gently rising target, then recover on solid ground. | Vertical drift replaces horizontal drift; no gate or timer. |
| 23. Beside the vortex | Broad left route or smaller reward landing, both reconnecting. | Familiar vortex beside a choice; no moving support. |
| 24. Fade, then focus | Leave a timed support for a smaller stable landing. | One deliberate precision step, followed by broad recovery. |
| 25. Two quiet openings | Read two gates separately with stable waiting supports. | Repeat timing decisions in sequence. |
| 26. A wider rhythm | Ride a large curve between broad destinations. | Deliberate recovery, with only one reward dot. |
| 27. Choose your crossing | Broad hexagon route or smaller circular bonus route. | Optional precision pays three dots; the fallback remains broad. |
| 28. Crossing the tide | Lead two landings drifting in opposing rhythms. | Familiar moving targets in a new diagonal arrangement. |
| 29. Wait, then leave | Wait for a gate, then later leave a fading support. | Gate and timer are separated by a stable landing. |
| 30. Find your flow | Curve, moving landing, then optional reward route and broad finish. | A short composition of known decisions. |

## Progression

Completion opens the next encounter immediately. Three failed attempts still offer the next encounter without awarding completion. Cumulative active time preserves access for players who struggle: the original twenty retain their thirty-second discovery schedule; encounter 21 becomes available at 600 seconds and each later depth encounter at sixty-second intervals, ending at 1,140 seconds. Waiting in menus or pausing does not count. Existing progress, best distance and per-encounter retry counts migrate from both six- and twenty-entry saves.

This thirty-encounter baseline originally used an integer completion mask. The later expanded prototype migrates it to a 64-bit completion field; see [EXPANDED_LEVEL_REVIEW.md](EXPANDED_LEVEL_REVIEW.md).

## Review questions

- Does a new arrangement create a different decision, rather than merely a different silhouette?
- Is vertical drift readable before takeoff?
- Do the reward branches make their extra precision worthwhile?
- Are the gate and crumble decisions separated clearly enough?
- Does encounter 26 provide a welcome change of rhythm?
- Does the finale feel like applying learned skills rather than an abrupt difficulty spike?

## Validation

The Unity Mac build succeeds. All 111 connections across thirty encounters pass shared-window checks across all ten characters, with 178 later motion samples. The ten new encounters contribute 37 connections. Their minimum shared main-route launch window is 0.322 seconds and their minimum optional-route window is 0.298 seconds. The minimum across the complete pack, including the preserved opening bonus route, remains 0.233 seconds.

An automated look-ahead controller completes each of the ten new encounters continuously across all ten characters (100 traversals), using actual waiting, orbiting, launch, flight, crumble timing, gate motion and lava pressure. It never teleports between supports. This demonstrates mechanical feasibility with simulation-perfect timing; it does not predict human completion rate or establish fun. Additional checks cover both save migrations, timed access, bounded instances, reward budgets, final landing completion, pause/reset behaviour and real hazard collisions.

The first twenty compare unchanged after accounting for JSON floating-point round-trip precision. Visual review of the Mac viewer confirmed distinct gate hatching, broad stable recovery supports and a visible final fork with a three-dot optional branch. User playtest of the new ten is pending.
