import type { LegStyle } from '../characters/catalog';
import { GAME } from './config';
import { nearestOnSegment, normalize } from './geometry';
import type { Vec } from './types';
export type OrbitPath = {
  points: Vec[];
  normals: Vec[];
  cumulative: number[];
  length: number;
};
export function createOrbit(outline: Vec[]): OrbitPath {
  'worklet';
  // Arc-length sampling prevents speed changes on lobes; a wider tangent smooths joins.
  const normals = outline.map((_, i) => {
    const a = outline[(i - 3 + outline.length) % outline.length],
      b = outline[(i + 3) % outline.length];
    return normalize({ x: b.y - a.y, y: a.x - b.x });
  });
  const points = outline.map((p, i) => ({
    x: p.x + normals[i].x * GAME.playerRadius,
    y: p.y + normals[i].y * GAME.playerRadius,
  }));
  const cumulative = [0];
  for (let i = 0; i < points.length; i++) {
    const a = points[i],
      b = points[(i + 1) % points.length];
    cumulative.push(cumulative[i] + Math.hypot(b.x - a.x, b.y - a.y));
  }
  return { points, normals, cumulative, length: cumulative[points.length] };
}
export function orbitPose(path: OrbitPath, distance: number) {
  'worklet';
  const d = ((distance % path.length) + path.length) % path.length;
  let lo = 0,
    hi = path.points.length - 1;
  while (lo < hi) {
    const mid = Math.floor((lo + hi + 1) / 2);
    if (path.cumulative[mid] <= d) lo = mid;
    else hi = mid - 1;
  }
  const i = lo,
    j = (i + 1) % path.points.length,
    t =
      (d - path.cumulative[i]) /
      (path.cumulative[i + 1] - path.cumulative[i] || 1);
  const a = path.points[i],
    b = path.points[j],
    n = path.normals[i],
    m = path.normals[j];
  const normal = normalize({
    x: n.x + (m.x - n.x) * t,
    y: n.y + (m.y - n.y) * t,
  });
  return {
    x: a.x + (b.x - a.x) * t,
    y: a.y + (b.y - a.y) * t,
    nx: normal.x,
    ny: normal.y,
  };
}
export function nearestOrbit(path: OrbitPath, p: Vec) {
  'worklet';
  let best = { distance: Infinity, along: 0 };
  for (let i = 0; i < path.points.length; i++) {
    const result = nearestOnSegment(
      p,
      path.points[i],
      path.points[(i + 1) % path.points.length],
    );
    if (result.distance < best.distance)
      best = {
        distance: result.distance,
        along:
          path.cumulative[i] +
          result.t * (path.cumulative[i + 1] - path.cumulative[i]),
      };
  }
  return best;
}

/** Support distance of the rotating teeth along the inward surface normal. */
export function gearContactRadius(
  nx: number,
  ny: number,
  gearAngle: number,
  legCount: number = GAME.legCount,
  legStyle: LegStyle = 'triangle',
) {
  'worklet';
  const pitch = (Math.PI * 2) / legCount;
  const inward = Math.atan2(-ny, -nx);
  if (legStyle === 'paddle') {
    const left =
      gearAngle -
      0.06 +
      Math.round((inward - gearAngle + 0.06) / pitch) * pitch;
    const right =
      gearAngle +
      0.06 +
      Math.round((inward - gearAngle - 0.06) / pitch) * pitch;
    return (
      GAME.playerRadius *
      Math.max(Math.cos(left - inward), Math.cos(right - inward))
    );
  }
  gearAngle += legStyle === 'blade' ? 0.045 : 0;
  const tooth = gearAngle + Math.round((inward - gearAngle) / pitch) * pitch;
  return GAME.playerRadius * Math.cos(tooth - inward);
}
export function groundedPose(
  pose: { x: number; y: number; nx: number; ny: number },
  gearAngle: number,
  legCount: number = GAME.legCount,
  legStyle: LegStyle = 'triangle',
) {
  'worklet';
  const correction =
    gearContactRadius(pose.nx, pose.ny, gearAngle, legCount, legStyle) -
    GAME.playerRadius;
  return {
    ...pose,
    x: pose.x + pose.nx * correction,
    y: pose.y + pose.ny * correction,
  };
}
