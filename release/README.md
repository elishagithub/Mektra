# Mektra release assets

## Latest submission: 1.5.0 (7)

Submitted to Google Play production review on 29 October 2026. Play Console confirmed **Changes in review** for the new release, description, icon, five phone screenshots and feature graphic. Managed publishing is off. Publication is pending Google's checks and approval.

Artifacts: `android/Mektra-1.5.0.aab` and `android/Mektra-1.5.0.apk`. Verification: `android/verification-1.5.0.txt`; submission record: `android/production-submission-1.5.0.txt`. New artwork: `store-assets/1.5.0/`; actual game captures: `screenshots/1.5.0/`; selected store images: `store-previews/1.5.0/`.

Glowing orb heroes with matching trails, sparse sparkles, elliptical flight and a takeoff pulse; faster jumps and 5% faster movement; Setting Sun, Invert, Black & White and Blue Hue; refreshed character selection and readable preset buttons.

## Previous submission

Package: `com.arkcodesolutions.mektra`
Submitted update: 1.4.0 (version code 6), sent to Google Play production review on 2026-10-25. Play Console confirmed Changes in review while automated checks run. Not yet live. Submission record: `android/production-submission-1.4.0.txt`.

Artifacts: `android/Mektra-1.4.0.aab` and `android/Mektra-1.4.0.apk`. Verification: `android/verification-1.4.0.txt`. Existing live version at preparation: 1.3.0 (5).


## Store artwork

- `store-assets/mektra-icon-512.png`: 512 × 512 RGBA PNG.
- `store-assets/mektra-feature-1024x500.png`: 1024 × 500 RGB PNG.
- Masters retain the original AI-generated artwork. Created with the built-in OpenAI image-generation tool, using actual Mektra gameplay as visual reference.

Icon: glossy black Mektra with luminous eyes, amber feather details and a cyan rim against the blue forest. Feature graphic: Mektra jumping between organic platforms in the firefly forest with a MEKTRA title. Generated with the built-in image tool using project-owned forest and character art as references; exported to 512×512 and 1024×500 respectively.

## Current game captures

`release/screenshots/1.4.0/` contains five authentic Mac game captures. `release/store-previews/1.4.0/` removes the window title bar and pads the sides to 1080×1920 without changing gameplay. Screens cover the main menu, characters, settings, gameplay and pause. These are actual current game screens, not generated UI mockups.

## Android validation

Signed ARM64 IL2CPP, min API 25, target API 36. Production Unity project ID is present in both archives. An existing 16 KB-page-size Android emulator installed and launched the APK. The check caught a stripped MeshCollider used by runtime-created quads; `Assets/link.xml` now preserves it and the corrected startup is clean. Real leaderboard read was verified against the original production board; Mac UI verified the genuine best-score row.

## Signing

The new private upload key and its password file are in `signing/`, excluded from Git. Back up both securely: the same upload identity is needed for future updates. Never publish this folder or password. Alias: `mektra-upload`. RSA 4096 / PKCS12.

## Release behavior

All retries now begin at the opening. The temporary death-section replay branch has been removed.
