import type { LevelDefinition, Platform, Star } from './types';
export function spawnSection(
  level: LevelDefinition,
  section: number,
): { platforms: Platform[]; stars: Star[] } {
  'worklet';
  const offset = -section * level.stride;
  return {
    platforms: level.platforms.map((_, index) => ({
      key: `${section}:p:${index}`,
      index,
      section,
      offset,
    })),
    stars: level.stars.map(s => ({
      ...s,
      y: s.y + offset,
      key: `${section}:s:${s.id}`,
      section,
      collected: false,
    })),
  };
}
