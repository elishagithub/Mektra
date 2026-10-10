import { GAME } from '../engine/config';
import type { LegStyle } from '../characters/catalog';
export const ROBOT_LEG_COUNT = GAME.legCount;
/** All silhouettes fit the same gameplay radius. Cached by the renderer. */
export function robotLegPath(count: number, style: LegStyle) {
  const point = (radius: number, angle: number) =>
    `${(Math.cos(angle) * radius).toFixed(3)} ${(
      Math.sin(angle) * radius
    ).toFixed(3)}`;
  const base = GAME.legBaseRadius,
    tip = base + GAME.legHeight;
  return Array.from({ length: count }, (_, i) => {
    const a = (i * Math.PI * 2) / count;
    if (style === 'paddle')
      return `M${point(base, a - 0.1)}L${point(tip, a - 0.06)}L${point(
        tip,
        a + 0.06,
      )}L${point(base, a + 0.1)}Z`;
    if (style === 'blade')
      return `M${point(base, a - 0.13)}L${point(tip, a + 0.045)}L${point(
        base,
        a + 0.095,
      )}Z`;
    const width = style === 'needle' ? 0.04 : 0.095;
    return `M${point(base, a - width)}L${point(tip, a)}L${point(
      base,
      a + width,
    )}Z`;
  }).join(' ');
}
export const ROBOT_LEGS = robotLegPath(ROBOT_LEG_COUNT, 'triangle');
export const ROBOT_SHELLS = {
  circle: 'M14 0A14 14 0 1 1 -14 0A14 14 0 1 1 14 0Z',
  roundedSquare:
    'M-6 -12H6Q12 -12 12 -6V6Q12 12 6 12H-6Q-12 12 -12 6V-6Q-12 -12 -6 -12Z',
  hexagon: 'M0 -14L12.124 -7V7L0 14L-12.124 7V-7Z',
  capsule: 'M-4 -11H4A11 11 0 0 1 4 11H-4A11 11 0 0 1 -4 -11Z',
  diamond:
    'M0 -14Q2 -14 4 -12L12 -4Q14 -2 14 0Q14 2 12 4L4 12Q2 14 0 14Q-2 14 -4 12L-12 4Q-14 2 -14 0Q-14 -2 -12 -4L-4 -12Q-2 -14 0 -14Z',
};
