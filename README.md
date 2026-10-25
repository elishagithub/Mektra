# Mektra

Mektra is a portrait arcade climbing game. The active game and Android release are built in Unity 6000.3.25f1 from `Unity/`. The older React Native prototype remains in this repository for reference; its documentation is in `README-react-native-prototype.md`.

## Gameplay

Tap to jump from the curved surface you are orbiting. Timing determines the straight launch direction. Climb continuously through 99 authored layouts with organic platforms, moving surfaces, rain streams, narrow bouncy routes and long diagonal jumps. Score is the highest upward distance reached, one point per 10 world units. Collect drifting fireflies to unlock animated characters. There is no health meter or energy drain. Retries begin at the opening.

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

Builds, Unity import caches, node_modules, raw gameplay captures and signing secrets are excluded from version control. Local progress remains on the device. Optional global rankings use Unity Authentication and Leaderboards; joining publishes a nickname and best score. The project also includes the LevelPlay SDK. See the privacy policy and release setup notes for service configuration.

Locally imported Amplify Color and Camera Play packages are excluded from this repository. Restore those dependencies from your licensed originals before compiling; the selected colour LUT resources are also local. Fonts include their license in `Unity/Assets/Resources/Fonts/`.

Version 1.4.0 (6) was submitted to Google Play review on 25 October 2026 with the restored production leaderboard link, refreshed forest UI and updated listing artwork.

## Sync changes

Commit reviewed source changes and push the current branch to the configured GitHub origin. Regenerate Unity caches locally rather than committing them.
