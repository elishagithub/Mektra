# Mektra

Mektra is a portrait arcade climbing game. The active game and Android release are built in Unity 6000.3.25f1 from `Unity/`. The older React Native prototype remains in this repository for reference; its documentation is in `README-react-native-prototype.md`.

## Gameplay

Tap to jump from the curved surface you are orbiting. Timing determines the straight launch direction. Climb through 21 worlds with four authored courses per world: circles, lobes, smooth curves, hexagonal spaces, moving capsule rows, narrow bouncy routes, wedges, zigzag passages and giant lobes. Distance is the run score. Stars replenish health and buy characters using local game currency. Progression follows the authored journey; death retries begin at the opening.

## Source and editing

- `Unity/Assets/Scripts/`: simulation, rendering, audio, UI and build scripts.
- `Unity/Assets/Resources/`: active level packs and character definitions.
- `assets/levels/`: authored level data and source revisions.
- `figma/`: editable world exports and review tools.
- `scripts/`: level-generation, audit and export helpers.
- `release/store-assets/`: store artwork.
- `release/privacy-site/`: published privacy-policy source.

Open `Unity/` in Unity 6000.3.25f1. The scene is `Assets/Scenes/Mektra.unity`. Android package: `com.arkcodesolutions.mektra`.

## Release build

The editor entry point is `Mektra.Editor.MektraAndroidRelease.Build`. It builds a signed ARM64 APK and AAB. Required dependencies: Android Build Support, SDK API 36/build tools 36, NDK r27c, JDK 17. The current local script configures these installed tool paths; adjust them for another machine.

Signing material lives in `release/signing/`, intentionally excluded from Git. Keep the upload key and password in a secure backup and restore them locally before signing an update. Increase `PlayerSettings.Android.bundleVersionCode` for every Play Store update.

Builds, Unity import caches, node_modules, local gameplay captures and signing secrets are excluded from version control. The current game has no account, ads, analytics or network data uploads; progress is stored locally.

## Sync changes

Commit reviewed source changes and push the current branch to the configured GitHub origin. Regenerate Unity caches locally rather than committing them.
