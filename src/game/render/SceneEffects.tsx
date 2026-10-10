import React from 'react';
import { Circle, Group, Path, Skia } from '@shopify/react-native-skia';
import { useDerivedValue, type SharedValue } from 'react-native-reanimated';
import { GAME } from '../engine/config';
import type { GameFrame } from '../engine/types';

type Props = { scene: SharedValue<GameFrame> };
export function PlayerTrail({ scene }: Props) {
  const path = useDerivedValue(() => {
    const builder = Skia.PathBuilder.Make();
    const frame = scene.value,
      points = frame.trail,
      count = points.length;
    if (!frame.player.visible) return builder.detach();
    const start = 0,
      end = count;
    for (let i = start; i < end; i++) {
      const a = points[i],
        b = points[i + 1] ?? frame.player;
      const dx = b.x - a.x,
        dy = b.y - a.y,
        length = Math.hypot(dx, dy);
      if (length < 0.001) continue;
      const r =
        (1 + ((i + 1) / Math.max(1, count)) * 14 * GAME.playerScale) / 2;
      const nx = (-dy / length) * r,
        ny = (dx / length) * r;
      builder
        .moveTo(a.x + nx, a.y - frame.cameraY + ny)
        .lineTo(b.x + nx, b.y - frame.cameraY + ny)
        .lineTo(b.x - nx, b.y - frame.cameraY - ny)
        .lineTo(a.x - nx, a.y - frame.cameraY - ny)
        .close();
      builder.addCircle(b.x, b.y - frame.cameraY, r, true);
    }
    return builder.detach();
  });
  return <Path path={path} color="#FFFFFF" opacity={0.5} />;
}
function ParticleCloud({ scene, stars }: Props & { stars: boolean }) {
  const path = useDerivedValue(() => {
    const builder = Skia.PathBuilder.Make(),
      frame = scene.value;
    for (const particle of frame.particles) {
      if ((particle.kind === 'star') !== stars) continue;
      const r = particle.radius * particle.opacity,
        y = particle.y - frame.cameraY;
      if (r < 0.1) continue;
      if (!stars) {
        builder.addCircle(particle.x, y, r);
        continue;
      }
      for (let i = 0; i < 10; i++) {
        const angle = particle.rotation - Math.PI / 2 + (i * Math.PI) / 5;
        const radius = i % 2 ? r * 0.43 : r;
        const x = particle.x + Math.cos(angle) * radius,
          py = y + Math.sin(angle) * radius;
        if (i === 0) builder.moveTo(x, py);
        else builder.lineTo(x, py);
      }
      builder.close();
    }
    return builder.detach();
  });
  return (
    <Path path={path} color={stars ? '#BE8527' : '#303236'} opacity={0.85} />
  );
}
function BurstRing({ scene, index }: Props & { index: number }) {
  const transform = useDerivedValue(() => {
    const b = scene.value.bursts[index];
    return [
      { translateX: b?.x ?? -100 },
      { translateY: (b?.y ?? -100) - scene.value.cameraY },
    ];
  });
  const r = useDerivedValue(() => scene.value.bursts[index]?.radius ?? 0);
  const opacity = useDerivedValue(
    () => scene.value.bursts[index]?.opacity ?? 0,
  );
  return (
    <Group transform={transform} opacity={opacity}>
      <Circle
        cx={0}
        cy={0}
        r={r}
        style="stroke"
        strokeWidth={1.5}
        color="#BE8527"
      />
      <Circle cx={0} cy={0} r={r} color="white" opacity={0.12} />
    </Group>
  );
}
/** Batch many particles into two paths instead of hundreds of animated properties. */
export function SceneParticles({ scene }: Props) {
  return (
    <Group>
      <ParticleCloud scene={scene} stars={false} />
      <ParticleCloud scene={scene} stars />
      {[0, 1, 2, 3].map(index => (
        <BurstRing key={index} scene={scene} index={index} />
      ))}
    </Group>
  );
}
