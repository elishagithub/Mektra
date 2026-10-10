import type { Vec } from './types';
export const length = (v: Vec) => {
  'worklet';
  return Math.hypot(v.x, v.y);
};
export const normalize = (v: Vec): Vec => {
  'worklet';
  const m = length(v) || 1;
  return { x: v.x / m, y: v.y / m };
};
export function nearestOnSegment(
  p: Vec,
  a: Vec,
  b: Vec,
): { point: Vec; t: number; distance: number } {
  'worklet';
  const dx = b.x - a.x,
    dy = b.y - a.y;
  const t = Math.max(
    0,
    Math.min(
      1,
      ((p.x - a.x) * dx + (p.y - a.y) * dy) / (dx * dx + dy * dy || 1),
    ),
  );
  const point = { x: a.x + t * dx, y: a.y + t * dy };
  return { point, t, distance: Math.hypot(p.x - point.x, p.y - point.y) };
}
export function pointInPolygon(p: Vec, points: Vec[]): boolean {
  'worklet';
  let inside = false;
  for (let i = 0, j = points.length - 1; i < points.length; j = i++) {
    const a = points[i],
      b = points[j];
    if (
      a.y > p.y !== b.y > p.y &&
      p.x < ((b.x - a.x) * (p.y - a.y)) / (b.y - a.y) + a.x
    )
      inside = !inside;
  }
  return inside;
}
export function raySegment(
  origin: Vec,
  direction: Vec,
  a: Vec,
  b: Vec,
): number | null {
  'worklet';
  const sx = b.x - a.x,
    sy = b.y - a.y,
    den = direction.x * sy - direction.y * sx;
  if (Math.abs(den) < 1e-8) return null;
  const ax = a.x - origin.x,
    ay = a.y - origin.y;
  const t = (ax * sy - ay * sx) / den,
    u = (ax * direction.y - ay * direction.x) / den;
  return t > 0 && u >= 0 && u <= 1 ? t : null;
}
