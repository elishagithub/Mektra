# Shape variety and difficulty adjustment

Release remains on hold for user review. This is an adjustment to the existing 99-level game, preserving its mechanics and progression.

## Intent

The user found too many large circles and too little increase in challenge. Keep circles as an intentional part of the game, vary their sizes, and restore more two-lobe, three-lobe and distorted closed curves. Preserve the long diagonal launch as a central pleasure of the game.

## Implementation

- Levels 1–33 retain their accepted introduction and original rain identity.
- Levels 34–99 replace many ordinary circles with gently concave two-lobe and three-lobe outlines, or asymmetric closed circles. Remaining circles use several sizes. Moving-ferry shortcuts deliberately use clear small circular targets, with a little extra margin for judging the rendezvous.
- Ordinary landing scale tapers gradually toward the end of the campaign, up to 16% before local size variation. Small optional landings have a floor; checkpoints, starts, measured rebound entries and final arrivals keep generous envelopes.
- Some checkpoint circles gain a mild irregular curve without applying the ordinary landing-size taper. Broken rings still identify recovery points.
- Selected later cross-screen jumps rise 25–50 units farther and extend sideways by up to 10 units. Their launch and landing pads receive more margin than nearby short precision jumps. The first such launch retains a smooth circular pad; later curved departures require more care. The following checkpoint approach gives recovery.
- Rain capsules, rebound plates, broad catchers, gate rhythms and player physics retain their established rules. No new shapes with spikes or open C outlines are introduced.
- Level identities, connection graphs, checkpoint indices and saves are retained. Route previews and reward positions are re-authored against the actual revised geometry.

## Difficulty checks

The minimum authored shared launch window progresses from 0.25 seconds in levels 34–55, to 0.22 in 56–77, and 0.18 in 78–99. Optional branches use 0.20, 0.18 and 0.16 seconds respectively. These are acceptance floors across all ten characters, not claims that every jump has exactly that timing window. Later motion phases retain a readable opportunity within a bounded wait.

Continuous completion and every checkpoint restart must still pass. This catches impossible flights, grounded hazard contact, wall clipping and lava pacing failures. Automated results cannot establish human enjoyment; the stronger precision curve remains subject to the user's play review.

## Validation

The adjusted campaign passed all geometry and traversal checks:

- Levels 34–99 now contain 289 circles (previously 596), 110 two-lobe shapes, 83 three-lobe shapes, 135 distorted circles and nine ovals. Rain capsules and banks retain their established forms.
- Late ordinary landings have a median area-equivalent diameter 23% smaller than before. This compares physical polygon area; it does not imply every shape shrinks by the same amount.
- 41 connections are more than 15 units longer, with the selected cross-screen transfers receiving recovery margin.
- 652 fixed routes, 1,612 later motion checks and 272 live rain choices passed across ten characters. The narrowest authored shared window is 0.180 seconds.
- 660 full new-level runs and 1,430 checkpoint resumes passed. All 99 IDs, connection graphs and checkpoint indices remain unchanged; the first 33 levels match the previous build exactly.

Mac build succeeded. Native visual review confirmed the new curved silhouettes, mixed circle sizes and diagonal spacing in levels 80–81; the preview is open for the user. See `validation/SHAPE_BALANCE_VALIDATION.txt` for the check summary. The previous 99-level build is retained in `assets/levels/discovery/campaign-99-before-shape-balance.json` for comparison or reversal.
