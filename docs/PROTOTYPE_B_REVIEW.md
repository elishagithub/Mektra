# Prototype B review

Local review only. Release remains on hold until the user is satisfied.

The first six encounters are loaded from the approved Prototype A artifact without rebuilding their geometry or changing their rules. The additional fourteen use the same player, collider, jump speed and pickup reach.

| Encounter | Design purpose |
| --- | --- |
| 7. Rebound | A sloped bank and broad catcher introduce a physical ricochet. |
| 8. Leave before it fades | One timed support, with stable landings before and after. |
| 9. Read the opening | A moving gate introduces observing before committing. |
| 10. Honeycomb | Broad hexagon faces change the departure rhythm. |
| 11. Counterflow | Opposing rounded rows offer two crossings. |
| 12. Through the gap | Rounded hazard walls frame an open passage. |
| 13. The long jump | Spacious flight and large destinations. |
| 14. Change direction | Left and right turns reward reading ahead. |
| 15. Connected field | A broad route and a smaller optional crossing. |
| 16. Giant curves | A large organic shape changes the scale and rhythm. |
| 17. Quiet precision | A short framed crossing followed by recovery. |
| 18. Room to breathe | Broad landings and one reward give a deliberate rest. |
| 19. Familiar combination | Moving landing, stable rest, then a gate. |
| 20. First journey payoff | Familiar curves, an optional reward route and a broad finish. |

## Progress and review

Existing discoveries, scores, active time and retry counts carry forward. Completion unlocks the next encounter; three misses offer the next discovery. Active gameplay also unlocks one encounter every thirty seconds, putting encounter twenty within 570 active seconds across attempts. Paused time does not count. Unlocking does not mark an encounter complete.

The Journey menu scrolls through all twenty encounters. The player-free tour can inspect any encounter without changing progress. Review whether each new mechanic is readable, whether the recovery passages feel worthwhile, and whether the final combinations feel earned. Automated reachability checks cannot establish whether the levels are fun.

## Automated validation

Unity physics checks pass for 74 authored connections across all ten characters, plus 74 later motion samples. All main routes have shared launch windows of at least 0.30 seconds; the minimum including optional bonus routes is 0.233 seconds. The physical rebound has a separate 0.331-second shared window across all ten characters, and its dotted route follows an actual simulated rebound.

Checks also cover bounded encounter instances, sparse reward budgets, final-landing completion, preserved first-six data, migration of six-entry saves, unlock timing, pause and reset behaviour for crumble supports, real gate collisions and real vortex capture. These are isolated physics checks, not a claim of ten minutes of uninterrupted human play. User review of the fourteen new encounters is pending.
