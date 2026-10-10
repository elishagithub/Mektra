# Capsule rain

All four rain layouts (37–40) updated in Figma board 23:21169. Rain world 23:25240, level 37 23:25244. Previous curved board is hidden and preserved. Visual review at 50% confirmed black capsule silhouettes with semicircular ends, evenly spaced diagonal rows and points between them. Playfield backgrounds remain 390 × 5400; offscreen capsules extend group bounds. Figma reports cloud sync pending reconnection.

Unity rain authoring uses capsules 74 × 40, semicircular radius 20, tilt −15 degrees and row slope −0.10. Each three-capsule row has uniform spacing 520/3 and loops beyond the left/right edges. Row periods 24, 20, 27 and 22 seconds (variants 2/4 add 1.5 seconds); mild 18% flow variation. Motion holds during the introductory guidelines as before. Authored stars follow moving landings. Automated validation/build status is recorded after completion.

Validation: Experience regression passed (rider carry, speed variation, pause, offscreen recycling); 2624 connection checks, 1024 planner samples and 16 continuous 72-segment runs passed. Core 696 authored routes passed across eight characters. Mac build confirmed MEKTRA_BUILD_SUCCESS in /tmp/mektra-capsule-final.log. Independent row alignment/spacing check passed across 92 seconds including loop seams. Game app rebuilt, not relaunched. Figma cloud sync still pending.
