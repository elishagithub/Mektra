# Mektra: 25 four-screen levels

Each playfield is 390 × 3264 units (4 × 816). Start at the bottom and move upward. Named black platform paths and white star paths are editable in Figma. Screen dividers, player markers, wall highlights, and exit labels are authoring guides.

Import `Mektra-25-levels.svg` into Figma for the complete board, or import individual `Mektra_level_N.svg` files. SVG imports contain native editable vectors and text. JSON files contain the matching outlines, paths, stars, and tested launch distances. The existing long level is preserved.

Easy levels teach broad landing windows. Intermediate levels introduce shape changes and zigzags. Tough and critical levels shrink targets. Tension levels lean toward walls while preserving body clearance. Fun levels provide three stars per jump and recovery platforms. Every fourth platform and the start/end platforms are wider recovery zones.

`validation.txt` records actual Unity simulation checks across all eight characters. Difficulty tuning still requires human playtesting; route reachability does not establish subjective fairness. The pack is connected to the Unity level scheduler and cycles after level 25.

Regenerate geometry: Unity editor method `Mektra.Editor.LevelPackBuilder.Build`. Regenerate SVG: `python3 scripts/export-level-pack.py`.
