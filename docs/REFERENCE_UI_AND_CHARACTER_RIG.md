# Reference UI and articulated characters

25 October 2026. Local Mac review build. Release remains on hold.

The selected design direction is the user's misty-jungle reference UI, replacing the earlier Lumen/Ink/Aura exploration. Existing forest artwork remains in use. No generated replacement forest background is shipped for this update.

## Screens

- Main: spaced MEKTRA heading and luminous divider, organic silhouettes, animated character, ivory Play pill, round Characters/Ranks/Share/Settings controls.
- Characters: back control, spaced heading, animated central character, luminous pedestal, navigation arrows, five portraits per group, name and tagline, functional Select/Unlock action. All ten existing character IDs, ownership records and prices are retained, so the real counter is out of ten rather than the reference image's five.
- Settings: separate music and sound-effect volume sliders with percentages; persistent haptics preference; three background theme choices; colour-preset selector; ivory Done button. Vibrance is the default once, and later manual preset choices persist.
- Game over: real run distance, best distance and successful-jump count, animated character, glass statistics, Retry/Home/Share controls.
- Pause: spaced PAUSED heading, pause symbol, Take a breath caption, Resume, glass Restart/Settings rows and round Home control. Simulation and run timer stay paused while this screen or its Settings screen is open.
- Share: a composed result screen calls the existing native platform share integration only when the player chooses Share.
- Ranks: the existing Unity Services Authentication/Leaderboards integration and nickname opt-in flow are retained; no replacement ranking backend or fabricated live scores. Current build logs report an unlinked Unity cloud project. Connecting the Unity project is still required for online rankings.

## Character rig

Gameplay and selection render a real articulated 3D rig into transparent render textures. Body, eyes, feet, leg joints and feather/leaf details are separate meshes. Eyes blink and reopen independently, gaze follows movement during flight, alternating footfalls move the feet and their joints, flight tucks the feet, breathing and feeding affect body scale, and feather details sway. The right-hand firefly companion was removed from all rigs at the user's request. Feathers remain.

The existing collider, jump speed, platform geometry, continuous-run content and no-lava rule remain intact. Selection updates the actual gameplay avatar as well as the menu preview. Small carousel portraits render once rather than running six live animation cameras continuously. Desktop C/S/L shortcuts open Characters/Settings/Ranks from the home screen.

The visible core roster starts with Orbit (existing mektra ID), Ripple (tide), Ember, Aurora and Frost. Remaining existing companions stay available through the carousel. Eye palettes remain distinct for Volt, Nova, Bloom, Onyx and Comet too.

## Assets and licensing

Fonts are Raleway Light (300) and Medium (500), instantiated from the Google Fonts variable TTF; the open-font licence is included at `Unity/Assets/Resources/Fonts/Raleway-OFL.txt`.

Built-in image generation produced:

- `assets/design/character-study/orbit-reference.png`: character visual study extracted from the user's reference. It is not a flattened playable character.
- `Unity/Assets/Resources/Art/CharacterDesign/character-atlas.png`: visual study of five reference characters and the luminous pedestal. Only the pedestal region is used by the selection screen; runtime body/eyes/legs are separate 3D meshes.

Character study prompt: Extract only the large central glossy black creature and its yellow-green firefly companion from the supplied selection reference, preserving the spherical body, ivory eyes, cyan highlights and little glossy feet, centred on a transparent canvas, without pedestal, forest, text or UI. The companion in this historical visual study is not used by the current rig.

Atlas prompt: Create an isolated transparent 3-by-2 production asset sheet matching the five reference characters: original warm-eyed black creature, cyan-eyed black creature, amber-eyed creature with leaf antenna, violet-eyed creature with leaf antennae, pearly white creature with pale wings; the sixth cell contains the dark elliptical pedestal with a thin cyan luminous rim. Preserve glossy materials, identity and scale. No interface or lettering. This study predates the user's companion-removal correction; only its pedestal is used in the current UI.

## Validation

A GPU regression checks transparent avatar rendering, independent eye blink/reopen, foot gait and flight tuck. Existing frosted-forest, organic-platform, non-rain route, continuous-layout-join, pickup, no-health, no-lava and orbit-direction checks remain part of the build. Native Mac visual review covers the main menu and character carousel; final Settings/Pause/Result inspection is performed against the reference screenshots. Mobile-device haptics and live cloud rankings require their respective device/project configuration checks.

### Gameplay visibility
The black body keeps a broader, brighter cyan rim and a restrained cyan bloom so its silhouette remains readable against the forest and black platforms. Eye width increases from .245 to .275 and height from .285 to .31 in rig space; blink normalization follows the new height. Player size, collider, jump physics and collectible contact remain unchanged.

### Articulated forest bugs
Orbit and Onyx have eight spider-like legs; the other characters have six. Each leg has independent upper/lower segments, a raised knee and alternating foot lift. Legs fold inward during jumps. Antennae sway and react to flight; Aurora, Frost, Bloom and Comet have gently moving wings; Ember and Volt have separate beetle shell plates. Soft pastel eye, body-rim and halo colours distinguish the bugs using the firefly palette. The visual rigs do not alter player collision or jump physics. Existing character IDs, ownership and prices are preserved.

### Rounded crawling feet revision
The thin leg segments are replaced by two rows of spherical feet (10 or 14 total). A rearward-travelling phase wave staggers their stance and lift. Gameplay samples the actual supporting platform outline, including its motion, rotation and mirroring, to place feet against the local curve. Feet are independent of the body breathing transform, lift briefly on the return stroke and tuck during flight. Character collision and reach are unchanged.

### Glowing orb heroes (1.5.0)
Gameplay, hero portraits and character selection now use filled glowing orbs with character-coloured halos and trails. The default hero is white; existing character IDs, prices and unlocks are retained. Flight stretches the orb along its velocity and returns it to a circle on landing. Sparse trail sparkles remain, with a brief takeoff pulse and five launch streaks. Movement speed is 73.185 world units per second (5% above 69.7); jump speed is 660. Settings expose Setting Sun, Invert, Black & White and Blue Hue as four readable buttons.
