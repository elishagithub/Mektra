# Mektra

A portrait arcade game built with React Native 0.83.0 and React 19.2.0, matching the Shopify mobile project's core versions. Skia draws the game; Reanimated shared values update the canvas without React renders on every frame.

## Play

The Mini Robo player has an upright circular shell, a rounded face panel, cyan LED eyes, and 15 small triangle teeth that rotate like a mechanical gear. Eyes blink while idle, narrow during a jump, and smile briefly after landing. The body is 21 units across; collision clearance includes its 25.5-unit leg ring. Gear rotation stops on pause. Jumps travel at 720 units/second, with a 36-sample tapered trail. Source artwork lives in `assets/player/`.

The character walks automatically around each shape. The dots show its launch direction. Tap anywhere in the playfield to jump; wait until the dots point toward the next platform. Jumping follows a straight flight, like the earlier Mektra spark mechanic, with a small visible aim correction of at most 12 degrees toward the next platform. Landing attaches the character to that platform's outer curve. Collect black stars in flight. Contact with either screen side explodes the character, followed by the game-over menu. Drifting below the camera or flying away for seven seconds also ends the run.

The same authored section repeats forever. Its final-to-first transition is checked along with every other jump. Stars are banked after a finished run and can buy cosmetic characters. Progress is stored on the device; no server or account is required. Backgrounding pauses the game and requires a deliberate resume.

## Run

Use Node 20.19.4 or newer, Xcode for iOS, or Android Studio/JDK 17 for Android.

```sh
npm install
npm run ios:pods
npm start
```

In another terminal:

```sh
npm run ios:simulator -- "iPhone 18 Pro"
# or
npm run android
```

The repository includes the native `ios/` and `android/` projects. For a physical iPhone, set your own signing team in Xcode. Keep the phone and Metro host on the same network.

On Xcode 27, view the running simulator in Device Hub. The `ios:simulator` script uses `simctl` because React Native CLI 20 still expects the older `Simulator.app`. You can also open `ios/Mektra.xcworkspace` in Xcode and press Run. On older Xcode, `npm run ios -- --simulator "iPhone 18 Pro"` works normally.

## Structure

- `src/game/engine/`: pure TypeScript movement, collision, aiming, particles, and fixed-step simulation.
- `src/game/levels/`: authored level geometry and star positions. Rendering and movement use the same exported curves.
- `src/game/render/`: Skia drawing, independent of game rules.
- `src/game/hooks/`: frame loop and app lifecycle.
- `src/ui/`: reusable buttons, game-over/pause panels, and character shop.
- `src/storage/`: validated local progress and character catalog.
- `assets/levels/`: original Figma SVG export, balanced editable SVG, and verified launch plans.
- `scripts/`: repeatable SVG import and star placement tooling.
- `__tests__/`: all 29 jumps, star collection, side-wall death, pause, and repeated-section cleanup.

## Level authoring

The game level is [Mektra_level_1 in Figma](https://www.figma.com/design/3GMPeJH9DL1N4h7rQpF4TN/FarmToHomeRightBanner?node-id=2030-88). It is a 390 × 8500 frame with 29 white platforms and 34 black stars. Platform dimensions and clear vertical gaps are 25% larger than the previous version (typical gap: 107.5 units). Its artwork is `assets/levels/Mektra_level_1.svg`; the game data is `src/game/levels/level01.json`.

Edit that frame, keeping white shapes named `platform-*` and black stars named `star-*`. Export the whole frame as SVG, then import the saved file:

```sh
npm run level:import -- /absolute/path/to/Mektra_level_1.svg
npm run level:check-jumps
```

The importer preserves authored curves and star positions, applies nested Figma transforms, and calculates the repeat seam from the edited spacing. It handles paths, circles, ellipses, rectangles, and polygons. Keep the frame width at 390. The jump check uses the real game simulation and reports unreachable gaps or stars for you to adjust in Figma. Import does not automatically move stars or apply the 25% resize again. To import the checked-in SVG, run `npm run level:import` without a file argument. Later Figma edits need another export/import; there is no live sync.

`npm run player:design` regenerates the editable player expression SVG using the same 15-tooth geometry as the game. The updated [Mini Robo gear sheet](https://www.figma.com/design/3GMPeJH9DL1N4h7rQpF4TN/FarmToHomeRightBanner?node-id=2030-153) is beside the level in Figma.

Coordinates use Figma's downward Y direction with a 390-unit width. Sections repeat using a calculated stride rather than the full artboard height, preserving the gap across the seam. Old sections are removed once the player is safely beyond them. To add another level later, supply the same `LevelDefinition` schema and select it in the section spawner.

## Checks

```sh
npm run typecheck
npm test
npm run lint
```

The initial level passes 45 automated gameplay checks, including every platform transition for all eight characters, eight repeated-section seams, interpolation and gear contact. TypeScript and lint also pass. The iOS project includes the scene lifecycle required by Xcode 27 and the same `fmt` compiler workaround used in the Shopify app. Android native compilation and physical-device playtesting remain to be verified.

This is a first playable prototype. Device performance and timing should be tuned after hands-on play; automated reachability checks prove the route is playable but cannot judge how satisfying it feels. The simulation runs on the native UI runtime at a fixed 120 Hz. Display frames interpolate player position, camera and gear rotation between physics steps; Skia reads those UI-owned shared values. React receives only HUD changes and audio events. Only nearby objects are drawn. Decorative holes use even-odd fills; walking follows the exterior contour.

### Arcade audio and collection effects

Short original PCM sounds live in `assets/audio`, with native copies in the iOS target and Android raw resources. Run `npm run audio:generate` to regenerate all three locations. `GameAudio` preloads a small pool of native players; gameplay events are consumed once after each simulation update. The HUD music-note button mutes effects, and backgrounding the app stops playback. Adding the sound module requires reinstalling iOS pods and rebuilding the native app.

Trail segments and particle shapes are batched into a few Skia paths in `SceneEffects.tsx` to reduce animated property updates. Star pickups emit rotating star fragments and an expanding ring, and trigger the robot's happy expression.

Use `npm run ios:performance` for a bundled Release simulator build when assessing gameplay smoothness. It does not require Metro; `npm run ios:simulator` remains the Debug development workflow.


### Characters and motion checks

`src/game/characters/catalog.ts` defines eight cosmetic robots with different shells, teeth and eye styles. Default Mektra has a white shell and blue eyes. Every character uses the same jump speed and outer collision radius; rotating teeth determine the grounded contact offset. The single batched white trail uses 50% opacity. Character selection is a two-column vertical grid, and existing stars/purchases remain compatible with the saved progress format.

With the Debug app open and Metro running, `npm run test:native-ui` checks all 29 jumps and pause behavior on the actual UI runtime. It also stalls JavaScript for 300 ms and checks that the app's UI frame callbacks continue. This is an opt-in development check and does not alter the player's run or wallet. `GameEngine` remains a JS facade over the same worklet simulation for tests and level-authoring scripts.
