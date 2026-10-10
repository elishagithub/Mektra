import { raySegment } from './geometry';
import type { LevelDefinition, Vec } from './types';
/** Small, visible correction broadens tight launch windows without choosing a jump for you. */
export function assistedDirection(
  level: LevelDefinition,
  index: number,
  origin: Vec,
  normal: Vec,
  offset: number,
): Vec {
  'worklet';
  if (normal.y > -0.2) return normal;
  const next = (index + 1) % level.platforms.length,
    target = level.platforms[next];
  const targetOffset = offset - (next === 0 ? level.stride : 0);
  for (const degrees of [0, -3, 3, -6, 6, -9, 9, -12, 12]) {
    const a = (degrees * Math.PI) / 180,
      dir = {
        x: normal.x * Math.cos(a) - normal.y * Math.sin(a),
        y: normal.x * Math.sin(a) + normal.y * Math.cos(a),
      };
    for (let j = 0; j < target.outline.length; j++) {
      const p = target.outline[j],
        q = target.outline[(j + 1) % target.outline.length];
      const hit = raySegment(
        origin,
        dir,
        { x: p.x, y: p.y + targetOffset },
        { x: q.x, y: q.y + targetOffset },
      );
      if (hit !== null && hit < 480) return dir;
    }
  }
  return normal;
}
