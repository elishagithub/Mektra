# Mektra — researched roadmap from 33 to 99 levels

**Status:** All 99 levels are implemented and validated in the local Mac build. Levels 1–33 remain frozen. The six interleaved waves below are the design brief; see `CAMPAIGN_99_REVIEW.md` for the implemented layouts and validation results. Release remains on hold for user review.

**Baseline:** Preserve levels 1–33, including the corrected original rain streams. Release remains on hold until the user is satisfied.

## Design judgement

Yes, 99 is a credible target: 33 accepted levels plus 66 authored additions. We do not need 99 different mechanics. A small set of readable systems can produce depth through geometry, timing, route structure and combinations. Every addition must change a player decision; changing a title, colour, random seed or scale alone does not qualify.

Alternate families across the campaign. Avoid six rain levels followed by six bounce levels: that would repeat the delayed-variety problem from the old journey. Instead use **six waves of eleven levels**, revisiting each family once per wave.

## Internet research and how it applies

These are primary developer sources, a designer’s own project account, or publisher-authored store descriptions. The Mektra adaptations are our design proposals; they are not claims that another game uses our exact layouts. No success, retention or playtime conclusions are inferred from ratings or marketing copy.

| Reference | What the source actually supports | Proposed Mektra use |
| --- | --- | --- |
| [One More Line — SMG press kit](https://smgstudio.com/press/sheet.php?p=one_more_line) | A one-button timing game, developed through iteration from a slower rotation prototype. | Keep one-tap launch and test timing changes through a playable prototype before expanding them. |
| [Skyward — Ketchapp’s App Store description](https://apps.apple.com/us/app/skyward/id943273841) | Time a tap when a moving circle meets a tile; further patterns expand the play. | Design readable rendezvous between moving supports while keeping Mektra’s own normal-direction launch rule. |
| [INKS — level designer Steffan Glynn](https://www.steffanglynn.com/inks) | Distinct pinball puzzles, mechanics introduced in sections, forgiving curved surfaces early, and optional fewer-hit goals. | Use broad curved catches, deliberate bank sequences and optional fewer-launch mastery goals. |
| [holedown — grapefrukt press kit](https://presskit.grapefrukt.com/holedown/) | Planning ricochets and the satisfaction of a ball finding a productive path behind obstacles. | Build a few understandable multi-bank sequences with visible catchers; keep one player and no extra aiming input. |
| [Duet — Kumobius press kit](https://kumobius.com/presskits/duet.html) | Two orbiting spheres navigate increasingly complex falling obstacles through minimal controls. | Borrow the importance of readable relative motion and spacing; retain Mektra’s single player and established input. |
| [Orbit — HIGHKEY publisher description](https://apps.apple.com/us/app/orbit-playing-with-gravity/id991457817) | Stable orbits, attractive and repulsive gravity mechanics and pre-launch trajectory previews. | Explore orbital compositions and readable field boundaries. New force-altered flight is a separate experiment, not part of the committed 99-level route. |

## Eleven families

| Family | Engineering status | Distinct player decision |
| --- | --- | --- |
| Rain currents | Existing stream system | Slanted capsules, diagonal rows, off-screen recycling and sparse carried rewards. |
| Ricochet passages | Existing static banks; moving banks need a prototype | Read rebound angles, alternate banks and choose between a direct catcher and an optional chain. |
| Orbital carousels | Existing orbit motion; new grouped layouts | Transfer between supports circulating around a shared centre, with all immediate targets visible. |
| Gate rhythms | Existing moving hazards | Read alternating openings, paired shutters and visible waiting opportunities. |
| Ferries and elevators | Existing horizontal/vertical/orbit motion | Board a moving support, read its next rendezvous and depart toward a stable destination. |
| Linked mechanisms | New mechanic; prototype first | Landing on a clearly marked support opens a visibly linked gate; no additional input. |
| Rotating curves | New collision/render transform support; prototype first | Rotate ovals, lobes and rounded plates so the visible departure normal changes over time. |
| Wave paths | Existing phase offsets; figure-eight path is new | Moving supports form travelling waves; later introduce a bounded figure-eight with clear crossing order. |
| Geometric chambers | Existing shapes and hazards; new authored layouts | Read a compact room, choose a clear exit and cross between chambers using closed smooth shapes. |
| Vortex gardens | Existing lethal vortex and moving supports | Choose readable routes outside capture rings; use their spatial placement to change the crossing decision. |
| Chapter journeys | Existing systems plus only approved new ones | Compose two or three learned ideas with recovery, checkpoints and a clear final arrival. |

The new ideas with the largest technical differences are linked mechanisms, rotating non-circular supports, figure-eight paths and moving rebound plates. Existing orbital movement is not rotating collision geometry: rotating a perfect circle in place adds no useful decision.

## All 66 proposed additions

Part counts are pacing targets, not minimum length requirements. Wave 1 usually uses two parts, wave 2 two or three, wave 3 three, wave 4 three or four, and waves 5–6 mostly four. A compact encounter can stay shorter if more parts would repeat it. Longer levels retain checkpoint recovery. These are not predictions of human completion time.

### Levels 34–44: Introduction and recognition

| Level | Working name | Family | Main decision |
| --- | --- | --- | --- |
| 34 | The divided stream | Rain currents | Cross two visibly different rain speeds, resting between them. |
| 35 | The angled return | Ricochet passages | Read a new bank angle and catch a broad return landing. |
| 36 | Three travelling circles | Orbital carousels | Transfer between three circles orbiting a shared centre. |
| 37 | Alternating doors | Gate rhythms | Watch two openings alternate from a safe waiting support. |
| 38 | The patient ferry | Ferries and elevators | Board a lateral ferry, then depart when it approaches a stable exit. |
| 39 | A visible connection | Linked mechanisms | Land on a marked support and watch its linked gate open. |
| 40 | The turning oval | Rotating curves | Watch a slowly rotating oval change the available departure direction. |
| 41 | A travelling wave | Wave paths | Cross a phased vertical wave of rounded supports. |
| 42 | The open chamber | Geometric chambers | Read a broad chamber with two obvious exits that reconnect. |
| 43 | Around the ring | Vortex gardens | Follow a broad route around one plainly marked capture boundary. |
| 44 | A new current | Chapter journeys | Compose rain and a stable rebound in separate parts. |

### Levels 45–55: Variation and timing

| Level | Working name | Family | Main decision |
| --- | --- | --- | --- |
| 45 | Rain between islands | Rain currents | Use stable islands to choose which passing row to board. |
| 46 | Bank and ferry | Ricochet passages | Rebound to a broad slowly translating catcher. |
| 47 | The stretched orbit | Orbital carousels | Read a carousel whose elliptical path changes meeting distances. |
| 48 | The second opening | Gate rhythms | Depart through one gate, recover, then read a differently phased gate. |
| 49 | Two elevators | Ferries and elevators | Transfer between vertical ferries at their nearest approach. |
| 50 | Choose the door | Linked mechanisms | Two marked supports open distinct visible routes to a common exit. |
| 51 | The turning lobe | Rotating curves | Use an organic surface whose rotating lobes change the launch normal. |
| 52 | The crossing loop | Wave paths | Introduce one bounded figure-eight support between stable circles. |
| 53 | The side doorway | Geometric chambers | Traverse a short lateral doorway before recovering upward. |
| 54 | A moving safe route | Vortex gardens | Lead a moving landing while staying outside a distant vortex ring. |
| 55 | Meet, then commit | Chapter journeys | Compose an orbital meeting and a separate gate decision. |

### Levels 56–66: Choice and payoff

| Level | Working name | Family | Main decision |
| --- | --- | --- | --- |
| 56 | The reward current | Rain currents | Choose a broad rain crossing or a smaller reward-bearing capsule route. |
| 57 | The extra rebound | Ricochet passages | Broad direct catcher or a clearly optional third reflection. |
| 58 | Outer orbit, inner reward | Orbital carousels | Choose a broad outer transfer or a smaller inner landing; rejoin visibly. |
| 59 | Wide door, quick door | Gate rhythms | Choose a broad later opening or a shorter optional opening. |
| 60 | The ferry shortcut | Ferries and elevators | A ferry offers a short crossing beside a longer stable route. |
| 61 | Open your route | Linked mechanisms | Choose the switch that opens the route you intend to take. |
| 62 | Choose the shoulder | Rotating curves | Use a broad rotating shoulder or a smaller optional lobe. |
| 63 | Across the wave | Wave paths | Choose a spacious crest crossing or a smaller optional middle transfer. |
| 64 | Two ways through | Geometric chambers | Two chamber exits differ in launch count and landing size. |
| 65 | The outside reward | Vortex gardens | A reward detour stays visibly outside the capture boundary; broad route remains available. |
| 66 | Your way across | Chapter journeys | Compose a ferry, a branch and a broad common finish. |

### Levels 67–77: Sequential combinations

| Level | Working name | Family | Main decision |
| --- | --- | --- | --- |
| 67 | Rain to bank | Rain currents | Leave a rain row for a static bank, followed by a broad catcher. |
| 68 | A bank in motion | Ricochet passages | Introduce one slow translating rebound plate with measured physical reflection. |
| 69 | Carousel and doorway | Orbital carousels | Leave a carousel for a stable support, then read a shutter. |
| 70 | Wait, cross, recover | Gate rhythms | Sequence a gate and a later crumble support with stable recovery between them. |
| 71 | The passing meeting | Ferries and elevators | Transfer between a horizontal ferry and an arc-moving support. |
| 72 | The linked passage | Linked mechanisms | Open two successive gates from separate visible switch supports. |
| 73 | A rocking plate | Rotating curves | Read one slowly rocking rebound plate; no input during its bounce. |
| 74 | Wave and window | Wave paths | Cross a wave, settle, then pass a gate. |
| 75 | A connected pair | Geometric chambers | Two small chambers change the crossing direction before a broad arrival. |
| 76 | Orbit beside a vortex | Vortex gardens | Read an orbital transfer whose safe arc remains outside a capture boundary. |
| 77 | A composed passage | Chapter journeys | Combine a linked gate, a stable curve and a rebound with recovery. |

### Levels 78–88: Longer journeys

| Level | Working name | Family | Main decision |
| --- | --- | --- | --- |
| 78 | The rain archipelago | Rain currents | Four rain parts with stable islands, changing currents and checkpoint recovery. |
| 79 | The rebound gallery | Ricochet passages | Four measured bank passages with different entry angles and broad catches. |
| 80 | The carousel passage | Orbital carousels | Four orbital rooms, including a broad recovery room. |
| 81 | The breathing corridor | Gate rhythms | A long sequence of separate gate decisions with safe waiting supports. |
| 82 | A journey by ferry | Ferries and elevators | Ferry, elevator, recovery island and a final moving transfer. |
| 83 | The connected machine | Linked mechanisms | A visible sequence of switches and gates across several rooms. |
| 84 | The turning garden | Rotating curves | Rotating ovals and lobes separated by static checkpoint circles. |
| 85 | The wave passage | Wave paths | Travelling waves and one approved figure-eight transfer with stable resets. |
| 86 | Through the chambers | Geometric chambers | Several compact geometric rooms linked by readable passages. |
| 87 | The vortex garden | Vortex gardens | A longer route around separated capture rings, with broad recovery islands. |
| 88 | The long crossing | Chapter journeys | Compose three approved families in four connected parts. |

### Levels 89–99: Mastery and finale

| Level | Working name | Family | Main decision |
| --- | --- | --- | --- |
| 89 | Read the rain | Rain currents | Use familiar streams with optional precision; retain the original slanted-capsule identity. |
| 90 | The quiet ricochet | Ricochet passages | A measured rebound sequence with an optional harder chain. |
| 91 | The final rendezvous | Orbital carousels | Lead orbital meetings; give an open recovery route between them. |
| 92 | Doors in sequence | Gate rhythms | Apply learned gate rhythms without simultaneous blind decisions. |
| 93 | The last ferry | Ferries and elevators | Read a sequence of moving rendezvous, with a longer broad fallback. |
| 94 | The complete machine | Linked mechanisms | Use clearly linked switches in a sequence understandable from the visible rooms. |
| 95 | The changing horizon | Rotating curves | Apply learned rotating-surface normals without changing player speed. |
| 96 | The final wave | Wave paths | Read a wave and an approved crossing loop, then finish on a broad circle. |
| 97 | The open labyrinth | Geometric chambers | Choose a path through several visible rooms; every branch reconnects. |
| 98 | Beyond the vortices | Vortex gardens | Navigate separated vortex boundaries with a clear broad final approach. |
| 99 | Mektra: the full journey | Chapter journeys | A celebratory sequence of the strongest reviewed ideas, four checkpointed parts and a generous final landing. |

## First implementation batch

1. Preserve a reviewed snapshot of the accepted 33 levels.
2. Make four unnumbered mechanic studies: a three-circle carousel, a landing-activated linked gate, a rotating oval and a moving rebound plate. Show each with broad landings and stable recovery.
3. Test a figure-eight path as a fifth study only after its self-crossing motion is readable.
4. Author levels 34–39 as the first numbered batch after the relevant studies pass: rain, ricochet, carousel, gate rhythm, ferry and linked mechanism.
5. Author 40–44 after the rotation and wave studies are accepted. The user subsequently requested finishing all 99. Implement every wave now, keep regression checks, and retain the release hold for review.

If a study feels confusing, revise it or replace its reserved family slots with a different reviewed idea. Do not manufacture six versions merely to preserve a weak mechanic or hit the target count. Keep the same eleven-family interleaving when substituting.

## Fairness and identity rules

- Keep the black background, transparent grey outlines, small white dots, white player and blue trail.
- Keep player size, forgiving collider, orbit speed and ordinary launch speed consistent.
- Preserve thin diagonal rain and its off-screen recycling. Avoid both teleport collisions and phantom reward pickups at wrap boundaries.
- Keep narrow or multi-bounce routes optional until their intended trajectory and broad catcher are clearly understood.
- Rotating surfaces must show their orientation; update collision outline, surface normal, orbit position and rendering at the same simulation time.
- Moving bounce plates need reflection in the moving surface’s frame; do not silently inject or remove speed without a defined tested rule.
- Switches use a distinct monochrome marker and visible link. Broken outer rings continue to mean checkpoints, not switches or hazards.
- Gates remain visibly distinct through hatching. Do not use invisible solidity or a mandatory colour rule.
- Every long level has recovery and checkpoint boundaries. A checkpoint resets the attempt without awarding skipped score or distance.
- Route choices should reconnect visibly; the camera must show enough of a room to understand a lateral or return transfer.
- Do not turn every link into a reward lane. Start with roughly two rewards per part; concentrate a small extra cluster only on a deliberate optional route.
- Avoid increasing target precision, hazard speed and punishment together. Level 99 should celebrate learned skills with a broad final landing.

## Engineering needed before 99 is playable

### Progress storage

Implemented: stable completed level IDs and per-level checkpoint records support 99 entries. The legacy integer and 64-bit fields migrate through the original preference key. The playable count is 99. Currency, best score, discoveries and retries retain their existing storage.

Test completion of levels 63, 64 and 99 independently from levels 1, 32 and 33. Test migration from the six-, twenty-, thirty- and current thirty-three-level saves, including the rain checkpoint conversion. No existing progress should be lost or incorrectly marked complete.

### Authoring and validation

Replace validation assumptions tied to exactly 33 courses with a versioned manifest and per-family budgets. Separate fixed-route windows, dynamic next-row rain options, physical rebound sequences and stateful switch routes in reports. A list of possible graph edges is not evidence that every edge is reachable at every clock.

For every new level check actual fixed-step launches across supported characters, later motion phases, continuous completion, every checkpoint restart, bounds and moving collision clearance. For rotating and linked mechanisms also check pause, retry, checkpoint state, rendered/collision agreement and no unavoidable transition death.

The existing minute-based availability formula must be reviewed before extending it to 99. Successful completion should continue opening the next level immediately. Repeated failure should offer another discovery without falsely marking completion. Do not add forced waiting to lengthen the game.

## What determines the final length

99 levels alone does not establish campaign longevity. Record actual first-clear times, retries, optional-route use and where people stop. Keep these observations local. After that we can judge the campaign length, an optional clean-run/fewer-launch goal and a later endless mode. No retention estimate or total-hour promise is justified by the research or automated controller.

## Baseline audit

- Current playable count: 33.
- Accepted artifact: `assets/levels/discovery/expanded-prototype.json`.
- Artifact SHA-256 at planning time: `758503295bf23d30bc9212aecf1fbcf063d2c535cf131ae3fe63efcba011997f`.
- Levels 34–99: design proposals only; no runtime levels or save schema changed for this research task.
