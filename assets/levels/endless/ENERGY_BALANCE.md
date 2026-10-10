# Climbing energy

The live UI enables energy for every gameplay attempt. New attempts start at 100; drain starts after 25 active seconds (separate from the retained journey clock). Drain is 0.65 points/second; jumps after grace cost 1. Below 20, surface speed scales linearly from 100% to a minimum of 80%. Launch velocity and wedge reflections are unchanged, preserving existing jump reach. Zero energy ends the run. Pause does not drain.

Collected stars restore 4 energy, capped at 100, and still award one character-currency star. The collected flag prevents repeat energy rewards; magnet-delivered stars use the same collection logic. Blue filled blooms restore 40, capped at 100. In the endless journey, one bloom appears on the final landing every six sections. Standalone course previews retain refills on every second support. They sit on the upper traversable orbit, inside the screen margin, move with their support, and disappear with a vanished support. Their swept collection radius is player radius plus 7; they do not award stars or respond to star magnets. Old streamed pickups are trimmed.

Distance is the prominent score in metres; stars remain the character currency. Energy appears below distance, using ten rounded square glyph cells with layered white glow, a compact energy emblem, and a red 1 Hz pulse at critical levels. The remaining energy and percentage stay visible during the pulse. No labels are placed on gameplay shapes.

EnergyRegression checks refill supports in every course, slowdown, unchanged launch reach, star energy/currency rewards, no duplicate rewards, the energy cap, refill contact, restored speed, pause and drain for all eight characters. EndlessRegression runs 16 continuous 72-section safe/bonus campaigns with energy enabled; its predictor copies energy and collected refills.

This is the initial tuning pass. Refills reward taking time around a landing; skipping them increases survival pressure. Human testing should assess whether the density is too generous, and whether the 20% critical slowdown feels distinct enough.
