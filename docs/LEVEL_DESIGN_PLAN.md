# Mektra — level design plan

**Version:** 0.1 · 24 October 2026  
**Status:** Proposed design baseline; revise through playable reviews.  
**Release status:** ON HOLD. No release, production upload, or store submission until the user explicitly approves the game.

## 1. Design direction

Mektra should reward reading a space and choosing when to commit to a jump. Its identity is a small circle travelling around geometric surfaces, then launching straight along the surface normal. Keep that interaction simple and consistent.

The experience we want is:

**Notice an opening → choose a landing → time the launch → feel the arrival → discover a different situation.**

The first ten minutes should demonstrate the game's range. Later play should develop mastery of those familiar ideas. Variety must not depend on surviving one exceptionally long run.

This is a level and progression redesign, not a request for more mechanics or more shape families.

## 2. What the current build gets wrong

The source audit found:

- Four opening sections contain 84 platforms, before the shorter world sections begin.
- Black holes first appear in section 68 of the 84-section journey.
- Normal retries return to the opening, repeatedly delaying discovery for less experienced players.
- Rain sections have 56 connections. Automatically adding two to four rewards to every connection produces roughly 170 collectible dots in a section.
- Many different shape arrangements still ask players to make the same upward jump repeatedly.
- Rewards compete with route guides for attention instead of highlighting a few worthwhile choices.
- All characters currently render as the same white circle, weakening the visible payoff of character purchases.

Reordering the existing 84 sections alone will not fix these problems. Some sections need shortening, some overlapping themes should be merged, and retries need a different progression policy.

## 3. Decisions for the first prototype

| Area | Direction |
| --- | --- |
| Core movement | Keep orbiting, fixed-speed launches and perpendicular takeoff. |
| Art | Keep the black field, grey outlines, small white reward dots and blue player trail. |
| Player size | Preserve the current visible radius and the smaller forgiving collision radius. |
| Pickup reach | Require visible player/dot-core contact; decorative glow adds no reach. |
| Shapes | Reuse circles, ovals, rounded shapes, lobes and existing polygons. Keep C-shapes and spiked precision triangles removed. |
| Layout | Design short spatial encounters, alternating with readable recovery passages. |
| Discovery | Introduce the major mechanics within the first ten active minutes across attempts. |
| Failure | Reset the current attempt and score; preserve discovery progress. |
| Difficulty | Teach separately, repeat with a variation, then combine gradually. |
| Content | Reuse and edit existing content; do not preserve every old section merely to retain its count. |

Values and encounter timings below are initial tuning targets, not proven results.

## 4. First-ten-minute discovery route

Each row is a short encounter, usually three to five mandatory jumps. A mechanically simple encounter can be shorter. Do not add platforms or waiting solely to fill its time budget.

The times are the latest intended **availability windows across cumulative active play**, including failed attempts. They are not a requirement to survive continuously, and they are not timed teleports between sections. Skilled players may move through the route faster.

| Encounter | Availability target | Composition and player decision | First exposure limits |
| --- | --- | --- | --- |
| 01. First launch | 0:00–0:30 | Broad circle to broad circle; learn the departure direction. | Clear next target, no lethal obstacle, no bonus detour. |
| 02. Across the screen | 0:30–1:00 | A gentle diagonal crossing, then a broad return landing. | No moving targets; generous landing windows. |
| 03. Around the curve | 1:00–1:30 | Circle, rounded lobe and oval arranged as a small cluster. | Show how surfaces change the departure direction. |
| 04. A worthwhile choice | 1:30–2:00 | Broad safe route beside a smaller optional reward route; reconnect after one or two jumps. | Neither route looks like a dead end. |
| 05. Lead the landing | 2:00–2:30 | One slowly moving target between static supports. | Wide catcher; movement is visible before commitment. |
| 06. Rain crossing | 2:30–3:00 | A brief moving-shape stream with a few readable stepping stones. | Fewer active rows; two to four rewards in the entire encounter. |
| 07. First black hole | 3:00–3:30 | One clearly marked vortex beside a broad safe route. | No moving gate or crumble timer; safe route stays outside capture. |
| 08. Rebound | 3:30–4:00 | One short automatic wedge sequence, ending on a generous catcher. | No precision input during the locked rebound sequence. |
| 09. Leave before it fades | 4:00–4:30 | One crumbling support, followed by stable recovery. | Enough time to recognise the timer and make a deliberate jump. |
| 10. Read the opening | 4:30–5:00 | One slow gate crossing with a safe waiting support. | Lava must not force a blind launch while the gate is closed. |
| 11. Honeycomb | 5:00–5:30 | A short hexagon arrangement with a visible passage through it. | Broad entry/exit; no dense lethal field. |
| 12. Counterflow | 5:30–6:00 | A second, distinct movement rhythm using opposing streams. | Few objects; the key decision is timing a crossing. |
| 13. Through the gap | 6:00–6:30 | Rounded walls frame a short passage and a clear landing. | No spike-triangle squeeze; space around the arrival. |
| 14. The long jump | 6:30–7:00 | One open crossing with a large destination and a recovery landing. | Do not mistake distance for a tiny landing target. |
| 15. Change direction | 7:00–7:30 | Two deliberate zigzag turns using familiar shapes. | Preview the next turn before departure. |
| 16. Connected field | 7:30–8:00 | A compact connected-hexagon composition with a readable exit. | Distinct from encounter 11 through route structure. |
| 17. Giant curves | 8:00–8:30 | One large smooth surface beside smaller stepping stones. | Keep travel around the large surface brief; no long empty orbit. |
| 18. Quiet precision | 8:30–9:00 | A short, cleanly framed precision crossing with recovery afterward. | Static geometry; no simultaneous timer or moving hazard. |
| 19. Familiar combination | 9:00–9:30 | A moving landing followed by a separate gate decision. | Sequence the decisions; do not demand both at once. |
| 20. First journey payoff | 9:30–10:00 | A composed passage revisiting a curve, a choice and a broad final crossing. | A clear arrival and brief celebration; not a sudden difficulty wall. |

Organic curves and lobed crossroads are represented early rather than held back as separate long worlds. Reward-heavy “Star chimes” content should become selected reward routes, not another compulsory collectible corridor. Open stretches provide recovery throughout the route.

The ten-minute target means players get access to these encounters. Actually completing every encounter within ten minutes cannot be guaranteed for every skill level.

## 5. Progression and retries

### Discovery progress

- Remember which encounters have been introduced and completed, independently of run score.
- Completing an encounter opens the next immediately.
- Cumulative active play also opens the next scheduled discovery opportunity, so repeated failure does not indefinitely hide the game's variety.
- Never change the encounter underneath a player in flight.
- Offer a newly available encounter at a safe boundary or on the retry screen. Do not automatically skip an encounter without the player's choice.
- After repeated failures, make “Try next discovery” available alongside “Retry this encounter.” A skipped encounter remains available for practice.
- The player can return to introduced encounters through a compact journey view. This is access to practice, not a requirement to navigate a large menu before every retry.

### Checkpoints and score

- A failed attempt restarts at the safe entry to the current short encounter, with reset hazards and fresh opening grace.
- Death breaks the uninterrupted score/streak; it does not erase discovered encounters.
- A successful transition preserves the active run's score. Merely selecting a later encounter must not award the distance skipped.
- Keep the current currency. Avoid adding another progression currency.
- Any one-time completion bonus is paid once, not repeatedly by restarting the same encounter.
- Treat old high scores separately if route or scoring changes make them incomparable. Settle the score presentation before a release candidate.

For the first prototype, implement the short retry loop and discovery access first. Do not build an elaborate progression screen before proving that the encounters are enjoyable.

## 6. Rules for composing a screen

### Give each encounter one main idea

Write one sentence before placing shapes: “Cross to the moving target,” “Choose the bonus landing,” or “Pass outside the vortex.” If the encounter requires several unrelated explanations, simplify it.

### Use the width deliberately

- Mix diagonal crossings, brief lateral moves and upward recovery.
- Avoid a constant left-right-left-right ladder; predictable alternation is still repetition.
- Use occasional pairs or clusters of shapes at similar heights.
- Avoid mandatory backward detours during the introductory journey.
- A fork should visibly reconnect, usually within one or two jumps.
- Keep at least the next landing readable before takeoff; show a further landing where space permits.

### Keep a hierarchy

The current support and the next reachable target should dominate the composition. Other nearby shapes must have a purpose: alternate route, boundary, recovery or later destination. Do not add shapes merely to fill black space.

As an initial target, show about three to five relevant supports in an ordinary screenful. Rain and connected fields may exceed that, but should still present only one or two immediate decisions.

### Use size variation for a reason

Use roughly three ordinary size bands—small, medium and large—rather than random scaling on every platform. Start around radii 24, 40 and 60 in the 390-unit design space; evaluate non-circular shapes by their usable landing region. Giant shapes are occasional landmarks.

- Large supports establish safety and recovery.
- Medium supports carry the main route.
- Small supports offer optional precision or a brief later challenge.
- Avoid consecutive tiny mandatory landings during discovery.
- Keep landing regions at least 24 design units inside the actual visible viewport after zoom and safe-area adjustments.

Size bands and spacing must be validated with the actual orbit and launch mechanics. Bounding-box clearance alone does not establish playability.

## 7. Rewards and route guides

Permanent route dots already explain connectivity. Collectibles should create a reason to choose or time a jump, rather than duplicate every connection.

Initial reward budgets:

| Passage | Reward placement |
| --- | --- |
| Basic teaching jump | Zero or one dot when it reinforces the intended launch. |
| Ordinary short encounter | About three to six dots total. |
| Rain or counterflow introduction | Two to four dots total; never a lane on every graph edge. |
| Optional bonus route | A small, deliberate cluster of two or three dots. |
| Recovery passage | Zero to two easily readable dots. |
| Automatic rebound | A small reward sequence that follows the actual rebound path. |

- Remove blanket per-connection reward generation from the design policy.
- Preserve connection data for navigation without turning every possible connection into a collectible lane.
- Do not leave duplicate authored dots beneath generated ones.
- Space reward cores clearly apart from guide dots.
- Do not require collecting every dot to continue the journey.
- Reward placement should be checked in motion, especially on moving supports.

## 8. Difficulty after discovery

Increase the decision demand gradually. Do not make controls inconsistent or hide the next target to manufacture difficulty.

| Pass | Intended experience | Changes allowed |
| --- | --- | --- |
| Discovery | Recognise each idea and succeed with room to spare. | Broad landings, slow readable movement, isolated hazards. |
| Familiarity | Apply a known idea in a different arrangement. | Different crossing direction, one smaller landing, a modest movement variation. |
| Combination | Read two familiar ideas in a short sequence. | Moving support then gate; crumble then stable precision landing. |
| Mastery | Choose a harder route for a better payoff. | Narrower optional windows and more demanding combinations with a broad fallback route. |

Change one main difficulty variable at a time. Avoid shrinking targets, accelerating hazards and reducing recovery simultaneously.

Initial mandatory launch-window targets are approximately 0.35–0.65 seconds in discovery and 0.25–0.40 seconds in later ordinary passages. These must be measured in simulation and checked with human play. Very narrow windows belong on clearly optional mastery routes.

Keep the established player orbit speed, launch speed and forgiving collider consistent. Maintain recovery passages in every pass. “Later” must not mean uninterrupted punishment.

## 9. Hazard fairness and visual language

- Safe platforms use the existing single grey outline.
- Harmful objects need a distinct line treatment or animation; they must not look identical to safe supports in the monochrome palette.
- A black hole's capture boundary must be readable against black, including before the first approach. Do not rely on a black centre alone.
- Introduce each hazard with a broad safe route and a clear demonstration of its behaviour.
- Gate timings must provide a real opportunity to observe and commit.
- A crumble timer must allow recognition and departure, not demand foreknowledge.
- Lava pressure must leave time to read an introductory mechanic. Tune encounter entry grace and pressure from observed play, rather than disabling death or silently changing movement.
- No unavoidable collision at a section seam, motion reset or respawn.
- Never spawn a player inside a hazard's movement envelope.

## 10. What supplies involvement

The game needs more than a catalogue of mechanics. Every short encounter should supply a decision and a payoff.

1. **Anticipation:** show the next landing and make its opportunity readable.
2. **Agency:** occasionally offer a real safe/bonus choice, not several visually equivalent targets.
3. **Commitment:** let the player understand why a particular launch succeeds or fails.
4. **Arrival:** use restrained landing response and an audible confirmation. Reserve stronger feedback for a genuinely good sequence.
5. **Discovery:** give a new mechanic a recognisable entrance and a brief, uncluttered moment to appreciate it.

Keep the minimal visual direction. Differentiate encounters primarily through composition, scale and movement rhythm. Review sound and landing feedback after the first revised sequence is playable.

Cosmetic rewards should eventually have a visible effect on the circle or trail. Do not expand the character shop while its purchases look identical in play. This is a supporting motivation task, not a prerequisite for testing the first encounters.

## 11. Prototype sequence

### Prototype A — prove the rhythm

Build only six short encounters: first launch, diagonal crossing, choice, moving target, sparse rain and black hole. Reuse current shapes and physics. Add the short retry loop and ensure the black hole is available early across attempts.

Review this playable sequence before rewriting all later content. The main question is whether it feels more deliberate and inviting than the existing opening.

### Prototype B — complete the discovery journey

Extend to the twenty encounters above. Add the remaining mechanics in gentle forms, sparse rewards and recovery. Update the player-free viewer to show the same revised progression.

### Prototype C — add depth

Author second-pass variations and selected combinations. Strengthen arrival feedback, clarify hazards, and give optional routes a worthwhile payoff. Do not generate difficulty by globally shrinking every platform.

### Prototype D — balance and release review

Test the whole journey on the intended phone viewport. Resolve unfair deaths and repeated confusion, then review progression, scoring and cosmetic rewards together. Passing automated tests does not remove the release hold.

## 12. Validation and review gates

### Mechanical checks

- Check actual fixed-step launches and landings, including moving endpoints and section seams.
- Validate introductory mandatory routes for the supported characters; do not assume a geometric ray guarantees a safe landing.
- Exercise retries, fresh hazard timers, progress preservation, pause and returning from menus.
- Verify every discovery encounter becomes available within the ten-active-minute budget, including a session with repeated failures.
- Check that skipped encounters award no distance and do not consume completion rewards.
- Enforce reward budgets and check moving-dot contact.
- Treat the existing full endless-route validation failure as unresolved work. A successful preview build is not full journey validation.

### Human play questions

- Can a new player identify the next reasonable landing without an explanation?
- Can the player explain a miss and name something to try differently?
- Does a new encounter feel different in its decisions, not just its silhouette?
- Are rain and counterflow readable without following a cloud of rewards?
- Does the first black hole create curiosity before frustration?
- Do successful landings and optional routes feel worthwhile?
- Does failure invite another attempt without making discovery feel lost?

Record observations locally during development. Do not add analytics or data uploads merely to answer these questions.

### Suggested evidence for each review

- A player-free tour of the revised encounters.
- Short real gameplay recordings showing both successes and misses.
- Time to first encounter and retry count for each new mechanic.
- A small change log separating observed problems from designer guesses.

The user decides whether each prototype is satisfying enough to expand. No review is considered passed merely because the build runs or all targets are technically reachable.

## 13. Revision method

For each revision, record:

| Observation | Hypothesis | Smallest useful change | Evidence after change | Decision |
| --- | --- | --- | --- | --- |
| What the player did or reported | Why we think it happened | One layout, pacing or feedback adjustment | What improved, stayed confusing or became worse | Keep, revise or revert |

Avoid adjusting movement, collider size, reward reach and layouts together in the same playtest. Keep a stable comparison build so we can judge the effect of each design change.

### Revision log

| Version | Date | Change |
| --- | --- | --- |
| 0.1 | 24 October 2026 | Initial redesign plan: short encounters, early discovery, persistent access, sparse rewards and staged difficulty. |

**Current deliverable:** Expanded prototype: the accepted thirty encounters plus three four-part levels with persistent checkpoints. See [EXPANDED_LEVEL_REVIEW.md](EXPANDED_LEVEL_REVIEW.md) for composition, progression and validation.

## Prototype A implementation status

The six-encounter prototype is implemented and built locally. Review instructions and measured validation results are in [PROTOTYPE_A_REVIEW.md](PROTOTYPE_A_REVIEW.md). The user approved all six encounters as the design baseline on 24 October 2026: “i felt all six are awesome.” The remaining fourteen discovery encounters are implemented in Prototype B; the user approved their variations and authorised ten depth encounters for Prototype C. This approval applies to Prototype A, not to release.

## Prototype B priorities

Preserve the accepted first six encounters and their movement, collider, pickup reach and retry rules. Extend with gentle rebounds, crumble timing, gates, honeycomb and connected fields, counterflow, rounded passages, long jumps, zigzags, giant curves, deliberate recovery and a final familiar-mechanic combination. Keep rewards sparse and the complete discovery route available within ten active minutes across attempts. Adapt the remaining plan slots to avoid repeating introductions already covered by Prototype A; keep the total at twenty distinct encounters. Increase difficulty only after the discovery journey is established.

## Prototype C implementation

The user approved expanding after Prototype B on 24 October 2026. The ten-encounter depth batch is described in [PROTOTYPE_C_REVIEW.md](PROTOTYPE_C_REVIEW.md). Preserve the accepted twenty as a baseline, add familiarity and sequential combinations, keep optional precision on rewarding branches, and include a broad recovery encounter. This approval authorises implementation and local review, not release.

## Expanded-level experiment

After approving Prototype C, the user proposed deeper rain, wedge, narrow-plate and arc-moving-circle levels, with journeys around four times the existing size. The authorised next experiment preserves the thirty accepted encounters and adds three four-part levels with stable checkpoints. See [EXPANDED_LEVEL_REVIEW.md](EXPANDED_LEVEL_REVIEW.md). Evaluate these before expanding all thirty; do not use extra height or waiting alone to manufacture playtime.

## Roadmap toward 99 levels

The user accepted the corrected thirty-three-level build as good so far, then requested internet research and ideas for a campaign of 99. [ROADMAP_99_LEVELS.md](ROADMAP_99_LEVELS.md) maps 66 proposed additions across six interleaved waves, includes primary-source research, distinguishes existing systems from mechanics needing prototypes, and identifies save migration required beyond the current completion-bit limit. The user subsequently requested completing all 99. All 66 additions are now implemented and validated in the local Mac build; see [CAMPAIGN_99_REVIEW.md](CAMPAIGN_99_REVIEW.md) for current status. The first 33 remain frozen, and release remains on hold.
