# Expanded levels — three four-part prototypes

Local review only. Release remains on hold. The accepted thirty encounters stay as the opening foundation; this prototype adds levels 31–33 rather than replacing or enlarging their geometry.

“Four times the size” means a longer playable journey with several connected decisions. The player, shapes, collision radius, jump speed and pickup reach retain their existing scale. Each new level has a 4,000-unit authored canvas, four parts and three intermediate checkpoints. There are no menus or teleports between parts during a successful run.

| Level | Part 1 | Part 2 | Part 3 | Part 4 |
| --- | --- | --- | --- | --- |
| 31. A journey through rain | Original thin, slanted capsule streams | Different row speeds | Read the faster middle row | Cross the final stream to a broad checkpoint |
| 32. The rebound passage | Broad bank and catcher | Mirrored bank with a slightly smaller catcher | Two narrow plates forming a physical rebound chain | Return-side thin plate and a broad finish |
| 33. Circles in motion | Transfer between circles on orbital paths | A near-horizontal meeting between two arc-moving circles | Safe transfer or smaller reward branch | A diagonal arc sequence and broad finish |

## Checkpoints and retries

A broken outer ring identifies stable checkpoint supports. Landing on one saves the next part immediately, displays a brief checkpoint message and refreshes lava entry grace without interrupting flight or changing the motion clock. The HUD shows the level and current part.

A failed attempt can restart at the latest reached checkpoint, with fresh hazard clocks and support timers. Checkpoint progress survives quitting. Restarting at a checkpoint starts attempt score and distance at zero, awards no skipped distance, and removes already passed reward dots from that attempt. A separate “Restart level from beginning” action allows a full attempt. Completing the level clears its checkpoint so practice can start fresh.

Save migration retains discoveries, completion, retries, currency and best distance from the earlier six-, twenty- and thirty-encounter builds. Completion now uses a 64-bit field while reading the old thirty-bit mask, avoiding collisions between completion flags at level 33. The current format supports up to 63 encounters; extend storage again before exceeding that range.

The original access schedule remains unchanged. In addition to completion and three-miss access, active time makes level 31 available at 1,200 seconds, level 32 at 1,260 seconds and level 33 at 1,320 seconds. Menu and paused time do not count.

## What to judge

- Does each part ask for a fresh decision?
- Do the wide checkpoint rings make the longer journey inviting?
- Do thin bouncing plates remain understandable, with no midflight input required?
- Does arc movement produce interesting transfers without hiding the next landing?
- Do reward branches justify their extra precision?
- Is the longer journey satisfying throughout, or does a part feel like filler?

The rain revision restores the original `ExperiencePackBuilder.Rain` mechanic: 74×40 slanted capsules flow left and slightly downward, in diagonal rows with independent periods, and recycle beyond the screen edges. Each part has three rows with five evenly spaced capsules per row, plus a stable checkpoint. Row spacing is 175 units, matching the original stepping rhythm. Five capsules per 520-unit loop provide more landing alternatives than the original uneven four-capsule layout. Only two moving reward dots appear in each part.

Permanent route guides select a visible next-row target instead of drawing every possible connection. The player can choose other capsules in that row. The first rain version's part checkpoints migrate to the corresponding revised parts, rather than restarting earned progress. The first thirty levels and the expanded rebound and orbital-circle levels stay unchanged.

Rebounds are physical reflections with guides measured from actual trajectories. Arc-moving circles translate along orbital paths; rotating a perfect circle in place would not create a new decision.

## Validation

The original expanded build passed its fixed-route and checkpoint tests, but user review identified that its rain mechanic did not match the earlier game. That version remains available as `expanded-prototype-v1.json`; its successful physics checks did not establish fidelity to the intended design.

For the corrected build, validation separates fixed routes from dynamic rain choices: 133 fixed connections, 240 next-row rain alternatives, 64 shared row-choice windows at multiple timings, and 310 later fixed-route timing checks. The full expanded set passes 30 uninterrupted automated traversals and 90 checkpoint resumes across all ten characters. These are simulation-perfect feasibility checks, not human playtest times.

The rain now has sixteen main-route launches across four parts. Rebounds retain eight launches and the two-plate chain; orbital circles retain twelve main-route launches and their reward branch. Longer journeys are intended to create more decisions; they do not guarantee a particular human completion time.

## Rain correction — user review

The user reported that the first expanded rain was unlike the earlier rain. The comparison confirmed that gentle oscillation of large upright shapes had replaced the defining thin capsule streams. That version is kept in `expanded-prototype-v1.json` for comparison. The correction restores the stream mechanic and retains four parts, saved checkpoints and sparse rewards. The corrected build validates 64 live row-choice windows across all ten characters at four motion timings, with a minimum shared window of 0.312 seconds. It retains eight rewards total, checks that recycling happens outside both edges, and tests that a recycling reward cannot produce a phantom pickup. All three expanded levels pass 30 continuous runs and 90 checkpoint-resume runs across all ten characters. The old rain checkpoints map to the same earned parts in the new layout. The other thirty-two levels compare unchanged within JSON numerical round-trip precision.

## Accepted baseline

After the corrected rain build, the user said “all is good, so far.” Preserve these 33 as the accepted comparison baseline for further work. The researched 99-level proposal is in [ROADMAP_99_LEVELS.md](ROADMAP_99_LEVELS.md). Release remains on hold.
