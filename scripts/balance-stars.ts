import fs from 'node:fs';
import { level01 } from '../src/game/levels';
import { createOrbit, orbitPose } from '../src/game/engine/orbit';
import { raySegment, nearestOnSegment } from '../src/game/engine/geometry';
import { GAME } from '../src/game/engine/config';
import { GameEngine } from '../src/game/engine/GameEngine';
const plans = [];
const plansOnly = process.argv.includes('--plans-only');
for (let i = 0; i < level01.platforms.length; i++) {
  const a = level01.platforms[i],
    b = level01.platforms[(i + 1) % level01.platforms.length];
  const offset = i === level01.platforms.length - 1 ? -level01.stride : 0;
  const path = createOrbit(a.outline),
    candidates = [];
  for (let d = 0; d < path.length; d += 2) {
    const pose = orbitPose(path, d),
      dir = { x: pose.nx, y: pose.ny };
    if (dir.y > -0.28) continue;
    const origin = { x: pose.x, y: pose.y };
    let t = Infinity;
    for (let j = 0; j < b.outline.length; j++) {
      const p = b.outline[j],
        q = b.outline[(j + 1) % b.outline.length];
      const hit = raySegment(
        origin,
        dir,
        { x: p.x, y: p.y + offset },
        { x: q.x, y: q.y + offset },
      );
      if (hit !== null) t = Math.min(t, hit);
    }
    if (!Number.isFinite(t) || t > 480) continue;
    const x = pose.x + dir.x * t;
    if (x < 25 || x > 365) continue;
    const cx = (b.bounds.minX + b.bounds.maxX) / 2;
    // Imported stars belong to the author. Prefer a launch through their lane;
    // checking the level must never silently reposition collectibles.
    const laneStars = plansOnly
      ? level01.stars.filter(
          s => s.y < a.bounds.minY && s.y > b.bounds.maxY + offset,
        )
      : [];
    const starPenalty = laneStars.length
      ? Math.min(
          ...laneStars.map(
            s =>
              nearestOnSegment(s, origin, { x, y: pose.y + dir.y * t })
                .distance,
          ),
        ) * 20
      : 0;
    candidates.push({
      distance: d,
      origin,
      direction: dir,
      flight: t,
      quality: Math.abs(x - cx) + Math.abs(dir.x) * 10 + starPenalty,
    });
  }
  if (!candidates.length)
    throw new Error(
      `No safe jump from ${i + 1} to ${
        ((i + 1) % level01.platforms.length) + 1
      }`,
    );
  candidates.sort((x, y) => x.quality - y.quality);
  // A ray hit alone can pass through a concave source or miss an edited star.
  // Verify the actual simulation before recording a reviewable launch plan.
  const best = candidates.find(candidate => {
    const engine = new GameEngine(level01);
    const pose = orbitPose(engine.orbits[i], candidate.distance);
    Object.assign(engine.player, pose, {
      distance: candidate.distance,
      platformKey: `0:p:${i}`,
      mode: 'orbit',
      flightTime: 0,
    });
    engine.setViewport(750);
    engine.start();
    engine.jump();
    for (
      let step = 0;
      step < 240 &&
      engine.player.mode === 'flight' &&
      engine.phase === 'playing';
      step++
    ) {
      engine.advance(GAME.fixedStep);
    }
    const target =
      i === level01.platforms.length - 1 ? '1:p:0' : `0:p:${i + 1}`;
    return (
      engine.phase === 'playing' &&
      engine.player.mode === 'orbit' &&
      engine.player.platformKey === target &&
      (!plansOnly || engine.score > 0)
    );
  });
  if (!best)
    throw new Error(
      `No playable jump with authored stars from platform ${
        i + 1
      }. Adjust this gap or its stars in Figma.`,
    );
  plans.push({
    from: i,
    to: (i + 1) % level01.platforms.length,
    ...best,
    window: (candidates.length * 2) / GAME.orbitSpeed,
  });
}
const stars = plans.flatMap((p, i) => {
  const count = p.flight > 140 ? 2 : 1;
  return Array.from({ length: count }, (_, j) => ({
    id: `star-${i + 1}-${j + 1}`,
    x: +(
      p.origin.x +
      (p.direction.x * p.flight * (j + 1)) / (count + 1)
    ).toFixed(3),
    y: +(
      p.origin.y +
      (p.direction.y * p.flight * (j + 1)) / (count + 1)
    ).toFixed(3),
  }));
});
const data = { ...level01, stars };
if (!plansOnly) {
  fs.writeFileSync(
    'src/game/levels/level01.json',
    JSON.stringify(data, null, 2) + '\n',
  );
}
fs.writeFileSync(
  'assets/levels/jump-plan.json',
  JSON.stringify(plans, null, 2) + '\n',
);
function starPath(x: number, y: number) {
  return (
    Array.from({ length: 10 }, (_, i) => {
      const a = -Math.PI / 2 + (i * Math.PI) / 5,
        r = i % 2 ? 4.8 : 11;
      return `${i ? 'L' : 'M'}${(x + Math.cos(a) * r).toFixed(3)} ${(
        y +
        Math.sin(a) * r
      ).toFixed(3)}`;
    }).join(' ') + ' Z'
  );
}
const svg = `<svg xmlns="http://www.w3.org/2000/svg" width="${
  data.width
}" height="${data.height}" viewBox="0 0 ${data.width} ${
  data.height
}"><rect width="${data.width}" height="${
  data.height
}" fill="#E2E6EB"/>${data.platforms
  .map(
    (p, i) =>
      `<path id="platform-${i + 1}" d="${
        p.d
      }" fill="#FFFFFF" fill-rule="evenodd"/>`,
  )
  .join('')}${stars
  .map(s => `<path id="${s.id}" d="${starPath(s.x, s.y)}" fill="#303236"/>`)
  .join('')}</svg>`;
if (!plansOnly) {
  fs.writeFileSync('assets/levels/Mektra_level_1.svg', svg);
  fs.writeFileSync('assets/levels/Mektra-Level-01.svg', svg);
}
console.log(
  `${
    plansOnly
      ? 'Preserved authored stars; checked'
      : `Balanced ${stars.length} stars across`
  } ${
    plans.length
  } verified jumps (including the repeat seam). Smallest launch window: ${Math.min(
    ...plans.map(p => p.window),
  ).toFixed(2)}s.`,
);
