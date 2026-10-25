# Mektra — campaign wave one review

This document describes the first eleven additions. The user subsequently requested the entire campaign; see `CAMPAIGN_99_REVIEW.md` for current status. Release remains on hold.

## The eleven additions

| Level | Encounter | Player decision |
| --- | --- | --- |
| 34 | The divided stream | Cross original slanted rain capsules at different current speeds, with a stable island halfway. |
| 35 | The angled return | Read two physical bank rebounds, including a slowly moving plate, into broad catchers. |
| 36 | Three travelling circles | Transfer between supports orbiting a shared centre before a stable checkpoint. |
| 37 | Alternating doors | Watch moving rounded gates and choose an opening from a broad support. |
| 38 | The patient ferry | Board a horizontal ferry, then work with vertical motion. |
| 39 | A visible connection | Land on a square-marked switch support to open its visibly linked gate. |
| 40 | The turning oval | Launch from the actual changing surface normal of a rotating oval. |
| 41 | A travelling wave | Read successive supports moving at different phases of a vertical wave. |
| 42 | The open chamber | Choose either side of a divider; the optional smaller landing rejoins the checkpoint. |
| 43 | Around the ring | Read vortex capture boundaries while following broad supports outside them. |
| 44 | A new current | Combine an original rain crossing with a measured bank rebound. |

Every addition contains two connected parts with a real midpoint checkpoint. Rewards stay sparse. Existing player size, collision radius and launch speed remain unchanged.

## Engine changes

Rotating supports now use the same transform for rendering, collision and launch direction. Moving plates reflect velocity relative to their own surface motion. Switch supports open only their linked gates; checkpoint retries restore upstream switches and leave upcoming switches closed. The moving-bank dotted guide is recomputed from a physical flight preview.

Progress now records completion and checkpoints by stable level IDs, with storage for 99 entries. Existing completion bits and checkpoint saves migrate using the same preference key. The first batch exposed 44 levels; the full campaign expands this to 99.

The approved first 33 levels are frozen in `assets/levels/discovery/accepted-33.json`; the builder verifies them before writing the new campaign pack. The original corrected rain layout is preserved.

## Review priorities

Automated route checks establish reachability, collision agreement and save behavior. They cannot establish whether a timing pattern feels satisfying. Review carousel transfers, switch readability, rotating-oval departure cues and moving-plate guidance before expanding the next wave. Human completion times and retention have not been measured.
