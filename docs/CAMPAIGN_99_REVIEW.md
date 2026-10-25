# Mektra — 99-level campaign review

**Release: ON HOLD for user review.** The full campaign contains 99 authored levels. The accepted first 33 remain frozen; 66 additions follow the eleven-family interleaving in the researched roadmap. The Mac build and final validation passed on 2026-10-24.

Latest shape and precision adjustments are documented in [SHAPE_AND_DIFFICULTY_REVIEW.md](SHAPE_AND_DIFFICULTY_REVIEW.md): fewer repeated circles, more lobed/irregular surfaces, variable landing sizes and longer selected diagonals.

## Campaign structure

| Levels | Purpose | Structure of additions |
| --- | --- | --- |
| 1–33 | Approved opening, discovery and expanded encounters | Preserved, including the corrected original rain |
| 34–44 | Introduce and recognize the new systems | Two connected parts per level |
| 45–55 | Change timing and relative motion | Two connected rooms per level |
| 56–66 | Add readable route choices | Three checkpointed rooms per level |
| 67–77 | Sequence learned mechanics | Four checkpointed rooms per level |
| 78–88 | Longer themed journeys | Four checkpointed rooms per level |
| 89–99 | Apply learned skills and finish | Four checkpointed rooms per level |

The additions contain 209 checkpointed parts. Their extra length comes from connected encounters, recovery islands and route decisions. Player size, collision radius, orbit speed and ordinary launch speed remain consistent.

## What is playable

- Original thin, diagonally slanted rain with independently paced rows, off-screen recycling and stable islands. Later reward capsules are slightly thinner; the broad crossing remains available. The final wave restores three-row passages, separated by islands.
- Physical bank reflections, translating catchers, moving plates and the reviewed double-bank passage. Bounces finish on broad catchers; input remains locked during flight. The last ricochet journey adds optional return landings after catchers.
- Shared carousels, elliptical rendezvous and separate broad/small orbital routes. Their travel areas remain visibly distinct.
- Rounded moving gates with different periods and phases, stable waiting supports, and a later crumble decision separated from the opening. The final corridor adds a third opening to each room.
- Horizontal ferries, vertical elevators, arc-moving supports and a small optional ferry shortcut beside a longer stable route.
- Landing-activated linked gates. Square markers identify switches; thin lines show their gate. Branch rooms open distinct doorways, and later rooms sequence multiple switches. The final machine includes moving switch supports.
- Rotating ovals and smooth lobes. A later bank rocks gently. Rendering, collision, grounded movement and departure direction share the same transform.
- Travelling waves and bounded figure-eight motion. The final wave pairs crossing loops in alternating rooms. The figure-eight is a support's path; the player still uses the same orbit and tap-to-launch controls.
- Lateral doorways and chambers with reconnecting optional branches. The final labyrinth offers additional shortcuts between doorway supports.
- Vortex gardens with visible capture boundaries, moving targets and separated recovery islands.

Level 99 composes rain, a linked mechanism, an orbital meeting and a bank rebound in four parts. Clearing it shows “Journey complete,” with practice and journey navigation available.

## Changes from the working roadmap

The roadmap names describe encounter goals, not 66 unique engines. Layouts reuse learned geometry with changes to motion, branch structure, hazard timing and room order. The stretched orbit uses two opposed circles on an elliptical path; the three-circle study remains level 36. Later orbital reward paths travel beside the broad orbit rather than overlapping it. The extra-rebound and quiet-ricochet journeys include the previously reviewed double-bank sequence; an unreviewed third-reflection challenge was not added merely to match the draft wording. Level 73 combines rotating curves and a gently rocking rebound bank. These are authored adaptations of the plan.

## Progress and safety of existing saves

Completion and checkpoint records use stable level IDs, with capacity for all 99 entries. Legacy six-, twenty-, thirty- and thirty-three-level saves migrate through the existing preference key. Currency and existing store data retain their original storage. High completion indices do not reuse lower bit positions. A checkpoint retry restores earlier linked gates, leaves upcoming gates closed and awards no skipped score or distance.

Completing a level immediately opens the next. Existing discovery-by-active-play and repeated-failure options remain; there is no new forced wait. The final level does not point to an unimplemented level 100.

## Validation

Passed on 2026-10-24:

- 652 new fixed routes across all ten characters, including required physical reflection counts.
- 1,612 later motion checks, with a readable opportunity within a six-second wait.
- 272 live rain next-row choice windows.
- 660 uninterrupted new-level runs and 1,430 checkpoint-resume runs.
- Linked-gate activation/reset/checkpoint state, rotation/collision agreement, normal launch speed and moving-frame elastic reflection.
- Six-, twenty-, thirty- and thirty-three-level save migration; independent completion at indices 62, 63 and 98; checkpoint records by stable ID.
- Frozen first 33 levels, 99 unique level IDs, no exact duplicate physical layouts among the 66 additions, and matching campaign/runtime JSON.
- Mac build succeeded. The player-free preview visibly shows section 99 / 99. Native visual checks covered the new rain and finale.

See `validation/CAMPAIGN_99_VALIDATION.txt` for the validation summary. Automated controllers measure physical reachability and state behavior, not human enjoyment or completion time. Moving routes are not claimed reachable at every instant. Real continuous runs and every checkpoint resume are checked separately.

Human first-clear times, retries, optional-route use, readability and enjoyment still need review. No campaign-hour or retention claim is made. The game remains a local preview.

## Files

- `assets/levels/discovery/accepted-33.json`: frozen approved baseline.
- `assets/levels/discovery/campaign-99.json`: complete authored campaign.
- `Unity/Assets/Resources/discovery-pack.json`: runtime campaign.
- `Unity/Builds/Mektra.app`: Mac preview.
- `ROADMAP_99_LEVELS.md`: research sources and original design brief.
