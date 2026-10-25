# Firefly forest world — artwork and playtest

Original project artwork created with the built-in image-generation tool on 25 October 2026. The user's reference illustrations informed mood and composition. Their files are not shipped as game assets.

## Saved artwork

- `Unity/Assets/Resources/Art/FireflyForest/moonlit-forest.png`
- `Unity/Assets/Resources/Art/FireflyForest/emerald-forest.png`

Both are portrait 724 × 2172 PNG files. The game crops a moving vertical window rather than stretching them. The shader adds restrained mist, slow vertical movement and gradual blending between the two scenes. Three independent procedural tree layers move at different rates. Background imagery has no collision and contains no decorative insects that could be confused with collectibles.

## Moonlit forest prompt

Use case: stylized-concept. Create an original premium mobile game background asset, tall portrait 1:3 aspect ratio, luminous enchanted forest at night. Inspired by layered illustrated night woods, not copying any existing picture. Deep midnight indigo, luminous teal mist, soft violet moonlit air, restrained warm amber haze in distant clearings. Delicate ancient twisting tree trunks, tall slender distant silhouettes, overlapping organic canopies and fern fronds concentrated on left and right outer edges; central 65 percent must stay spacious, quiet, medium-dark blue teal fog without trunks or objects so pitch-black gameplay platforms can be read distinctly. Atmospheric depth with many translucent receding layers, beautiful polished painterly vector-like illustration, soft volumetric moonlight from upper left, refined flowing curves. Vertical composition intended for slow upward scrolling: forest floor and rocks must NOT form a hard bottom horizon; canopy and trunks extend past top and bottom edges. No characters, insects, fireflies, stars, sparkles, text, logos, UI, platforms or watermarks. Background only; softly luminous centre, darkest border corners. Output one finished high resolution portrait forest background.

## Emerald forest prompt

Use case stylized-concept. Original premium mobile game background, portrait 1:3 aspect ratio. Enchanted night forest, beautiful polished painterly vector-like illustration. A tall vertical emerald and turquoise woodland sanctuary with dark plum ancient trunks, delicate fern fronds and small distant luminous mushrooms only along outermost left and right edges, layered receding graceful tree silhouettes. Soft cyan shafts of moonlight and pale golden mist behind trees. Keep centre 65 percent open luminous teal mist with very sparse low contrast distant trees, space for black gameplay shapes. Vertical upward climbing game: no forest floor, no horizontal ground horizon, all trunks continue past top/bottom boundaries. Colour palette deep emerald, teal, muted plum-violet shadows, delicate warm honey highlights. Magical yet calm; complex art confined to borders. No fireflies or insects, no stars, no characters, no UI, no symbols, no text, no logos, no watermarks. Render one finished high resolution tall forest background.

## Gameplay presentation

Platforms are fully opaque pitch black. Mektra keeps its original circular collider and gains six articulated legs, a subtle rim and luminous eyes. Legs walk against the current support normal and tuck during flight. Fireflies have softly layered glow, fluttering wings and independently seeded, bounded oscillating paths. Their rendered positions remain their actual pickup positions.

The continuous forest run has no health or glow reserve. Fireflies are collectible currency; collecting or missing them does not change movement or cause death. Score is the highest upward distance reached, floored to one point per 10 world units. Legacy energy mechanics are disabled in the continuous forest simulation.

Release remains on hold for player review.

## Smaller platforms and frosted forest treatment

Platforms are scaled to 65% width and height about their existing centres in runtime copies. Vector paths, collision outlines and bounds use the same transform. The authored files are preserved. Firefly size, player collider, world centres and movement amplitudes remain unchanged. Bounce launch metadata is remapped to the smaller perimeter.

The forest now uses a restrained frosted-material appearance. Two separable Gaussian blur passes run once per background at load time into 384 × 1152 render textures. The scrolling shader then adds an 8% blue translucent film, very slight saturation reduction and a broad diffuse sheen. Foreground gameplay and UI are excluded from blur. Render targets are released when the effect component is destroyed.

Research consulted: Apple's Human Interface Guidelines, Materials (https://developer.apple.com/design/human-interface-guidelines/materials), distinguishes standard blur/translucency for content from Liquid Glass for controls; Unity's Graphics.Blit documentation (https://docs.unity.com/en-us/engine/6000.7/script-reference/unityengine/graphics/blit) describes GPU shader processing into render targets and restoring the active render target. This is a custom Unity effect inspired by frosted iOS materials, not a native Apple material.

Checks added: 764 non-rain authored links including rebound routes at reduced size, 1,960 section-boundary flights, and a GPU blur readback test confirming that edges soften without losing uniform dark/light areas. Rain layouts also receive the size change; their full moving-row route suite has not been rerun for this profile.

## Lava removed

The continuous forest game disables lava simulation, contact death, drawing, ember emission and rising-lava warnings. There is no health depletion in the continuous run. A regression places an active lava front above Mektra and confirms that it neither advances nor kills the player in forest mode. Legacy isolated course tests retain their own lava behavior.

## Organic platforms

Perfect circular supports are replaced in runtime copies with closed, smooth organic silhouettes: two lobes, three lobes and asymmetric curved ovals. Existing noncircular authored platforms remain. Every replacement fits the same bounding box and retains its centre, motion, rotation and 65% size profile. Rebound launch supports use subtler three-lobed variations. Render paths and collision outlines share the same points. Bounce launch metadata is remapped to the new perimeter. A geometry check rejects any remaining perfectly circular platform; route and section-join checks run on the modified silhouettes. Circular light halos, insect eyes and physics radii are not platform geometry and remain unchanged.

### Canopy light and dust
The gameplay backdrop adds three soft diagonal light shafts and sixteen tiny drifting dust motes. They animate slowly with simulation time, with two parallax depths and edge fading. All are composed inside the background shader before colour grading, behind platforms, fireflies and Mektra. Dust has no bloom, wings, collision or score value, keeping collectible fireflies distinct.

### Firefly capture feedback
Actual pickup contact triggers an 84-world-unit expanding light pulse lasting .6 seconds, plus twelve luminous fragments sized 4–8 units for .6–.85 seconds. The effect is roughly twice the former visual size, pauses with gameplay and clears on restart. Pickup radius and scoring are unchanged; no persistent collection ring is shown.
