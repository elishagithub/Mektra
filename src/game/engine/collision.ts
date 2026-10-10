import { GAME } from './config';
import { nearestOnSegment, pointInPolygon } from './geometry';
import type { PlatformDefinition, Vec } from './types';
export function touchesPlatform(
  p: Vec,
  definition: PlatformDefinition,
  offset: number,
): boolean {
  'worklet';
  const local = { x: p.x, y: p.y - offset },
    b = definition.bounds,
    r = GAME.playerRadius;
  if (
    local.x < b.minX - r ||
    local.x > b.maxX + r ||
    local.y < b.minY - r ||
    local.y > b.maxY + r
  )
    return false;
  if (pointInPolygon(local, definition.outline)) return true;
  for (let i = 0; i < definition.outline.length; i++) {
    if (
      nearestOnSegment(
        local,
        definition.outline[i],
        definition.outline[(i + 1) % definition.outline.length],
      ).distance <= r
    )
      return true;
  }
  return false;
}
export function touchesWall(p: Vec): boolean {
  'worklet';
  return p.x - GAME.playerRadius <= 0 || p.x + GAME.playerRadius >= GAME.width;
}
export function collectedOnStep(star: Vec, from: Vec, to: Vec): boolean {
  'worklet';
  // Swept collection catches a star even when a fast jump crosses it between frames.
  return nearestOnSegment(star, from, to).distance <= GAME.collectionRadius;
}
