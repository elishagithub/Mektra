# Edit Mektra endless sections in Figma

The active endless library contains **56 short section variants**, each a 390 × 1080 playfield. Source JSON and named editable SVGs are in `assets/levels/endless`. Sections appear automatically during one endless run; there is no picker. The earlier five-challenge and seven-world packs remain archived. Editable vectors are published in [Mektra — Endless Worlds](https://www.figma.com/design/HJLCW0yfd8iYcEcQ2kKz5l/Mektra-Endless-Worlds) using the personal Figma account open in Safari. The current boards are on two pages: **Later worlds · 28 layouts** (levels 29–56, board `4:2`) and **Opening library · 28 layouts** (levels 1–28, board `4:586`). Both boards match the current game source, including black rain and physical wedge reflections. Previous boards remain hidden for reference. The **Rain & wedge revisions** page also retains the eight focused rain/wedge layouts. All 56 layouts have separate named 390 × 1080 playfield groups, with individual platforms, hazards, stars and vortex cores as editable vector layers. Figma edits do not automatically change the game; export and import them using the steps below. Open `worlds-preview.html` to compare the 28 later-world variants, animate the streams and show the authored routes.

## Export and import

1. Edit a whole `Mektra_level_N` group (or convert it to a frame). Keep its 390 × 1080 size and stable object IDs.
2. Export SVG with **Include id attribute** enabled.
3. Run `npm run level:import-unity -- /absolute/path/to/Mektra_level_N.svg N`.
4. Run `npm run unity:build` and open `Unity/Builds/Mektra.app`.

The importer defaults to the active `endless` pack. An explicit third argument `challenges`, `worlds`, `pack-20` or `pack-25` targets a retained legacy pack. Endless imports preserve platform order and the branch connection graph: keep every platform ID intact. Crumble timers and labels are retained from source JSON. Imports stage edits and validate character routes plus continuous seeded endless runs with both fork policies before promoting them. Previous source data is saved in `assets/levels/import-staging/previous-pack.json`.

## Object IDs

- `platform-*`: walkable/landable black shapes or black rain drops.
- `star-*`: white collectible stars.
- `black-hole-*`: vortex core; moving or resizing this circle updates its center, core radius and proportional pull radius. The purple outline is a visual reference.
- `bounce-R-point-N`: mint waypoint marker for automatic wedge run R. Move these markers to adjust the ricochet path; keep the final marker on the exit support. Entry and exit platform indices and timing remain in JSON. The large opposing wedge banks touch the sides of the screen. Current wedge courses use elastic face-normal reflections, with a wide upper catcher followed by the circular section exit. There are no active bounce waypoint markers; the older waypoint section describes archived layouts.
- `hazard-*`: lethal black gates and corridor rails with orange edges. These never become landing platforms. Keep them separated from star and platform objects.

Export every hazard when editing gate or fork sections. If no hazard objects are present in an export, the importer preserves the existing rails rather than silently dropping collision geometry. Ordinary gameplay rebuilds do not replace authored vectors.

## Movement

Append metadata to a stable platform or hazard ID:

`hazard-gate-0__horizontal__110__0__3.2__0.25`

The fields are motion type, X amplitude, Y amplitude, period in seconds and phase in cycles. Supported types are `static`, `horizontal`, `vertical`, `orbit`, `stream` and `rain`. An optional seventh field stores speed variation (0–0.9), for example `platform-2__rain__520__34__25__0.67__0.55`. Rain uses one shared accelerating/decelerating clock, carries the player left, and resets only beyond the screen edges. The starting pose is preserved while the guides are visible. Stream motion travels left by the X amplitude and down by the Y amplitude each period, then resets; authored gaps protect the landing pockets. This example is a smooth horizontal ±110-unit gate with a 3.2-second cycle. Star attachment is `star-5__follow__platform-6`. Ordinary IDs preserve existing motion/attachment settings.

Player jump speed stays fixed and does not inherit platform velocity. Author receiving platforms for the source shape's outward normals: square faces, hexagonal faces and rhombus faces have different launch directions.

## Keep edits

`EndlessPackBuilder.Publish` regenerates the entire section library and is only for deliberate regeneration. Use the importer plus the ordinary build script for manual edits. `python3 scripts/export-editable-courses.py endless` refreshes exact editable SVGs from the current source pack.

Test narrow passages with the player's 19.602-unit collision diameter in mind. Gate sections have a 52-unit fixed tunnel gap. Preserve clearance, an obvious approach lane and a recovery landing after the corridor. Automated sampled reachability does not replace human playtesting.

Change connection topology, crumble deadlines or SAFE/BONUS labels in the JSON/builder, rather than renaming SVG objects. Endless timer behavior and graph data are gameplay logic, not exported vector geometry.

## Ordered world overview

The grouped overview (node `7:2417`) contains 16 named `World_N__Levels_A_B` groups, each holding four editable levels. Read left to right, then down. Levels 1–56 match the current game library; levels 57–64 are hollow-passage and connected-hexagon concepts. Countdown rings identify all source platforms with a crumble timer, and a separate visual-state strip shows the ring counting down and the platform disappearing. These are static editable behavior illustrations, not a running Figma game simulation. Prior boards are retained.

## Vertical collection

The main page is now **All worlds · 64 layouts**. Its editable collection is node `8:3652`; the first world is `8:3656`. All 16 world groups are stacked vertically. Each group contains four 390 × 1080 layouts stacked vertically in numerical order. Older separate boards on this page are hidden and retained for recovery. The SVG source is `Mektra_all_worlds_vertical.svg`. A four-level group contains 4,320 units of playable layout height, plus headings and spacing; an individual level is 1,080 units tall.

## Five-screen level designs

The current Figma collection is **Mektra_all_worlds_five_screens** on **All worlds · 64 layouts**. Every individual level is 390 × 5400, using five 390 × 1080 mobile screens. The first level is node `10:4895`. Shapes retain their dimensions; each original course pattern fills five screen runs. This changes the editable Figma designs; the live game pack has not been resized. The earlier 1080-high collection is hidden for recovery. Regenerate with `scripts/export-five-screen-worlds.py`.

## Current arrangement

The visible collection is **Mektra_world_columns**, node `10:15629`. The four levels within each world are vertical; all 16 world groups are arranged horizontally in numerical order. Every individual level remains 390 × 5400. Source: `Mektra_world_columns.svg`. Earlier arrangements are hidden.
