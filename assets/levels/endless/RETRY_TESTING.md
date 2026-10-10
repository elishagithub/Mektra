# Retry layout testing

SectionReplayMode.CycleAllLayoutsForTesting is enabled. Opening starts with world 0 / variant 0. Each retry advances the test itinerary: all 14 live world families, then the next authored variant. 56 retries cover all four variants of all 14 families. Progress section and elapsed time remain, while score, landing states and lava reset for a clean attempt. The itinerary loops after 56. It does not add the extra Figma-only concept worlds.

For later production selection, disable CycleAllLayoutsForTesting. InitialSeed then retains a positive random itinerary seed for a randomly chosen one to three runs. The existing normal progression controls world introductions and recovery beats. SectionReplayMode.Enabled controls whether replay stays at the current progress section.

Coverage is checked by ExperienceRegression at ten progress positions, alongside the normal endless campaign validation.
