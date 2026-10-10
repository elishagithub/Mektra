# Mektra release assets

Package: `com.arkcodesolutions.mektra`
Version: 1.0.0 (1)

## Store artwork

- `store-assets/mektra-icon-512.png`: 512 × 512 RGBA PNG.
- `store-assets/mektra-feature-1024x500.png`: 1024 × 500 RGB PNG.
- Masters retain the original AI-generated artwork. Created with the built-in OpenAI image-generation tool, using actual Mektra gameplay as visual reference.

Icon prompt: Premium minimal geometric arcade icon: black joined two-lobe curved orbital platform, tiny pink rounded-square robot with two white eyes, white curved jump streak toward a four-point star; amber atmosphere fading into petrol teal and charcoal. Full bleed square, no text, outer rounded corners, border, badges or UI. Keep key artwork within central 75%.

Banner prompt: Wide 1024:500 MEKTRA feature graphic; white geometric title at left, pink robot launching along a white curved streak between black joined two-lobe and hexagonal surfaces at right. Amber, petrol teal and dusty violet atmosphere, white four-point stars, subtle wedge passage. No device frame, UI, rankings, buttons, badges or additional text.

## Gameplay captures

Raw captures in `screenshots/` are authentic Mac gameplay captured while the user played. `store-previews/` removes the Mac title bar and pads the sides to 1080 × 1920 without changing gameplay. These are previews; Android-device screenshots should be captured after testing the APK.

## Signing

The new private upload key and its password file are in `signing/`, excluded from Git. Back up both securely: the same upload identity is needed for future updates. Never publish this folder or password. Alias: `mektra-upload`. RSA 4096 / PKCS12.

## Release behavior

All retries now begin at the opening. The temporary death-section replay branch has been removed.
