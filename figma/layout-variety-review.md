# Layout variety revision — 10 October 2026

Reviewed all 84 runtime layouts. Redesigned 80 layouts; retained the four recently reviewed giant-lobe layouts, levels 81–84. World order, testing checkpoints and frame positions remain unchanged.

The earlier repeated grammar was usually a mirrored alternating staircase, with equal vertical gaps and the same entry/exit circles. The new variants change approach direction, jump length, rest position and local clearance. The opening stays approachable; later variants mix commitment jumps with broad recovery landings rather than shrinking every target.

- Crossing, moving, crumble, gate and pressure worlds: four different spatial rhythms, unequal gaps and varied target sizes. Gates retain motion; precision rails occupy a long crossing with clearance from waiting surfaces.
- Star forks: safe and bonus branches sit at different heights, changing the timing and reward decision between variants. Shape labels are omitted.
- Spike, honeycomb, black-hole and organic worlds: different crossing rhythms, staggered landings and horizontal/diagonal transfers. Corridors and hazards follow the revised paths.
- Long leaps: different long diagonal launches and receiving positions; no mirrored duplicate route.
- Wedge springs, levels 45–48: straight taper, kinked shoulder, asymmetric waist and alternating banks; receiving capsules differ in width/thickness. Real reflection preserves speed and no scripted steering is added.
- Star chimes, levels 69–72: taper, offset elbow, hourglass and alternating passage silhouettes. The exits use a circle, capsule, authored organic surface and rounded square. The measured routes produce 6, 7, 6 and 3 physical rebounds; all trail stars are checked across eight characters.
- Rain: fully rounded black capsules retain close rows, independent speeds and offscreen recycling; the row spacing, slant and speed ordering vary by layout. Four stars per layout.
- Counterflow: neighbouring rows still move in opposite directions. Row spacing, capsule proportions, independent periods and reward-bearing columns vary.
- Hollow, connected-hexagon and zigzag worlds: introductory layouts have fewer, larger stepping surfaces. Later variants change long/short jump order and target proportions; obstacle lanes are derived around the authored rays.

`layout-variety-review.html` compares four actual runtime courses for each family, with one 390×1080 playfield per card. `assets/levels/endless/figma-editable` contains exact editable SVGs. The cloud atlas uses five consecutive courses from the actual fixed journey, climbing bottom to top, inside each existing 390×5400 preview frame. This removes the old five identical copies. Each screen is named with its actual course number; individual runtime sections remain 390×1080.

Validation is recorded in the Unity build log and `assets/levels/endless/campaign.json`. Physical checks establish reachability and survivable routes; player feedback is still needed to judge enjoyment.

Final validation: 16 continuous 128-section runs passed across eight characters and both safe/bonus policies. All 264 revised reward routes passed eight-character checks. Bounce trails, physical reflections, collisions, energy, lava and magnets passed; Mac build succeeded.

Figma atlas: 27:10492 at X0/Y0; bounce world 27:15516 at X13648/Y200; first bounce preview 27:15520. Previous roots 25:2 and 26:10020 are hidden and retained. All 21 columns remain in their original positions.
