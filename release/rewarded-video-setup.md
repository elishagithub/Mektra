# Mektra rewarded video setup

Publication is on hold until the owner checks gameplay and a rewarded test ad on Android. Do not submit the production release yet.

- Android package: `com.arkcodesolutions.mektra`
- LevelPlay app key: `289e6c1bd`
- Rewarded ad unit: `Mektra_Revive_Rewarded`, ID `esfioibwcx4fu3s3`
- Reward: one revive per run, full energy, paused until Continue
- New Arkcode Solutions Unity Ads Android game ID: `800394142`
- New active rewarded placement: `BP_Rewarded_Android`
- New Unity organization core ID: `13469982337887`
- SDK: LevelPlay Unity 9.3.2, native Android 9.3.0; Unity Ads adapter 5.4.0 / Ads SDK 4.16.5

The signed APK and AAB build succeeded. Revive regression passed across 84 courses and eight characters. APK signature and package were verified. This does not establish that a video serves on a real device.

## Android play check

The new Arkcode Solutions Monetization Stats reporting key was created and saved in LevelPlay with explicit owner permission. Unity Ads bidder instance `Mektra_Revive_UnityAds` maps Game ID `800394142` to placement `BP_Rewarded_Android`. The instance is active in the All Countries group of ad unit `esfioibwcx4fu3s3`. The default ironSource rewarded instance was deactivated. Credentials are not stored in source.

The updated signed APK and AAB built successfully. The APK was installed on the owner's connected LAVA LXX504 phone and launched. Unity test-device registration is pending the owner's advertising ID entry. Real video delivery and completed/skipped reward callbacks have not yet been verified. Do not claim these passed. The Mac launch currently renders a black window despite initialization completing; Android gameplay is the current play-check target.

Level validation passed 5,024 connection checks and 16 continuous 128-section runs across eight characters, with opening replay verified. Android logs show stripped MeshCollider warnings from decorative CreatePrimitive quads; investigate before publication.

## Pending before publication

Update the public privacy policy, Play Ads / Advertising ID / Data Safety declarations and listing for the actual SDKs. Reverify AAB signing and Android alignment. Refresh release checksums and verification notes. Commit audited source changes to GitHub. Select all countries and submit production only after the owner finishes their check.

## Account migration status

Arkcode Solutions owns the new Mektra Unity Ads app and rewarded placement. Both old organization deletion requests failed to complete in Unity; neither old organization is confirmed deleted. Do not delete or substitute credentials blindly.

Normal Replay and the old restart helper now both return to section zero. Even a command-line preview run resets to the opening on Replay. See `journey-order.md` for the complete 84-section order.

## Latest owner play-check changes

Player radius increased by 10% (9.801 → 10.7811). Compact horizontal health meter and familiar HUD controls are placed below Android safe-area/camera cutout. Lava grace shortened from eight seconds to four; its gradual entry ramp remains. The proposed rotation trigger was superseded by the owner’s four-second timing choice.
