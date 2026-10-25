# Continuous game flow

25 October 2026. Local playtest build; release remains on hold.

The regular game uses the existing main menu, pause menu, game-over screen, character shop, leaderboard and native Share action. The 99 authored layouts form one continuous ordered run. There is no level picker, level counter or completion screen between layouts. Death offers Play again from the beginning; the ending appears only after the final layout. Prior campaign review saves are preserved but are not used to skip the start of a new run.

Only the space between authored layouts changes: each next entry is placed 160 units above the preceding exit's upper edge. The layouts themselves and their internal routes remain intact. A single Simulation carries position, camera, score and elapsed time through the full sequence. Internal campaign checkpoint markers are hidden and do not create disconnected retry destinations in this mode.

Settings uses the existing monochrome styling and blue accent, with independent Sound effects and Background music buttons, Aim guide, an optional Change each replay toggle and 13 colour presets. The selected preset has a checkmark and accent. Choices persist automatically. Settings pauses an active run and resumes on Done or Escape.

The UI labels distance as distance, collectible currency as points, and shows the same run distance in results and Share. The existing leaderboard keeps its explicit opt-in nickname flow; no automatic enrolment or Share transmission is performed by this update.

Validation includes physical flights across all 98 joins, all characters and two motion clocks, in addition to the existing precision, pickup, gameplay and orbit-direction regressions. Colour LUT readability and both music loop assets are checked before the Mac build.

## Firefly point playtest

The dotted connections between shapes are hidden. Collectible points drift with independent, smooth sine cycles within 12 units horizontally and 9 vertically of their original anchors (15 units maximum displacement). The same moving position is used for rendering and swept pickup contact. The collectible size and pickup radius are unchanged. Parent platform motion is preserved. This drift applies to the continuous game; the authored layouts and static route review data remain intact. Validation checks motion bounds, speed, moving contact collection and rejection of distant points.
