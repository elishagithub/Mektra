# Mektra level review — 7 October 2026

Current runtime: 20 four-screen custom courses. All eight characters completed a continuous 222-jump run through all courses and the wraparound, collecting all 338 stars with lava and magnets active. This is deterministic automated playtesting, not a human enjoyment rating. Live game inspection checked the opening spacing, HUD and visual route clutter; automated UI input was not reliable enough for sustained manual play.

## Applied refinements

- Practice guides remain for 20 seconds, then fade smoothly until 25 seconds. Both the route and player-direction dots disappear together. Restart restores the practice period.
- Repositioned selected platforms by at most 48 units to remove awkward launch bottlenecks.
- Enlarged selected narrow targets uniformly, preserving their silhouettes and proportions. Every accepted edit retained or improved the shared incoming and outgoing jump windows.
- Repositioned every star lane on the middle of a contiguous launch window verified for all eight characters; reward counts and IDs are preserved.
- Walking and jump speeds, player size, magnet behavior, and jump whoosh gain remain unchanged.

Sampled windows are approximations from 360 orbit samples. They measure the longest contiguous safe landing interval shared by all characters, with magnets disabled for isolated route checks. They do not measure human success rates.

| Course | Title | Narrowest window before → after | Average window before → after |
|---|---|---|---|
| 1 | First Spark | 74 → 133 ms | 176 → 298 ms |
| 2 | Custom Crossing | 118 → 217 ms | 215 → 306 ms |
| 3 | Hook & Hollow | 154 → 198 ms | 243 → 256 ms |
| 4 | Playful Detour | 157 → 191 ms | 223 → 251 ms |
| 5 | Edge Timing | 124 → 169 ms | 153 → 194 ms |
| 6 | Sideways Rhythm | 88 → 127 ms | 147 → 297 ms |
| 7 | Narrow Passage | 74 → 162 ms | 171 → 225 ms |
| 8 | Three-Star Chase | 126 → 188 ms | 209 → 267 ms |
| 9 | Bent Metal | 137 → 174 ms | 235 → 253 ms |
| 10 | Tension Steps | 124 → 169 ms | 175 → 209 ms |
| 11 | Shape Carousel | 124 → 140 ms | 163 → 204 ms |
| 12 | Crossing Sparks | 59 → 108 ms | 157 → 202 ms |
| 13 | Precision Run | 95 → 95 ms | 129 → 142 ms |
| 14 | Breathing Room | 118 → 158 ms | 221 → 265 ms |
| 15 | Wall Whisper | 136 → 198 ms | 249 → 262 ms |
| 16 | Mechanical Maze | 135 → 169 ms | 176 → 206 ms |
| 17 | Critical Timing | 124 → 135 ms | 138 → 168 ms |
| 18 | Star Festival | 118 → 166 ms | 162 → 243 ms |
| 19 | Final Pressure | 99 → 124 ms | 171 → 184 ms |
| 20 | Mektra Master | 109 → 154 ms | 198 → 220 ms |

## Arcade references and application

- [Ketchapp Circle](https://apps.apple.com/us/app/circle/id911152486): one jumping action with a clear collision objective; its developer history includes race tracks and challenge modes. Mektra application: preserve simple input and make the next challenge legible.
- [Ketchapp Space Frontier](https://apps.apple.com/us/app/space-frontier/id1252963998): one-button play, repeated launches and earned upgrades. Mektra application: readable attempts, clear collection rewards, distinct progression milestones.
- [Ketchapp Stack](https://play.google.com/store/apps/details?id=com.ketchapp.stack): timing-focused tower building and best-score progression. Mektra application: retain precise timing, but remove accidental difficulty spikes.
- [Rayman Origins design account](https://www.gamedeveloper.com/design/rational-design-the-core-of-i-rayman-origins-i-): introduce mechanics gradually, vary input rhythm, and compensate when other pressures increase. Mektra application: teach, remix, introduce motion, test mastery, then provide a recovery landing.

These applications are design inferences, not claims that these games use Mektra’s proposed world structure.

## Shape-world roadmap

This is a proposed next pack, not the currently active geometry. Seven families × four courses gives 28 courses; the 20-course custom pack and its original backup remain separate.

| World | Courses | Skill |
|---|---|---|
| Circle Grove | 1–4 | Continuous surface normals and generous timing windows |
| Soft Square | 5–8 | Face direction and rounded-corner transitions |
| Hex Circuit | 9–12 | Repeated six-face launch directions |
| Rhombus Ridge | 13–16 | Diagonal normals and lateral crossings |
| Twin Lobe | 17–20 | Two convex launch regions and recessed turns |
| Tri Lobe | 21–24 | Three launch regions and variable orbit rhythm |
| Clover Core | 25–28 | Four-lobe timing, alternating routes and combined mastery |

Each world follows Learn → Remix → Motion → Mastery. Learn uses stationary broad targets; Remix changes gap rhythm and crossing direction; Motion first adds one slow horizontal mover, then vertical or orbital movement in later worlds; Mastery uses one short narrow path followed by a generous recovery landing. The first course of the next world resets complexity while retaining learned timing skill.

Narrow paths must be authored as actual obstacle collision boundaries, not decorative lines. Keep the initial corridor wider than the 17.82-unit player collision diameter plus a generous margin; tighten only after teaching alignment. Avoid combining unfamiliar lobes, moving targets, a tight corridor and severe lava pressure in the same introduction.

Flat-faced shapes cannot be laid out like circles: jump direction is the outward normal, so receiving platforms must match the face angles. Rounded corners provide a transition window, not steering. Stars and magnets should reinforce safe lanes, and no reward should lure the player into a lethal wall.

## Remaining design work

The new seven-world pack, moving-world progression and collision corridors are planned, not yet implemented. The current first course still contains a roughly 133 ms bottleneck despite its improved 298 ms average window; a dedicated circle-only teaching world is the better long-term solution. Course 13 retains the toughest measured 95 ms window and belongs in a late precision stage. Human playtesting is still needed to judge enjoyment and how quickly guidance should fade.

Exact updated editable SVGs are in `assets/levels/pack-20/figma-editable`. The existing remote Figma frames have not been overwritten.
