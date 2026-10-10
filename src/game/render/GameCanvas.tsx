import type { CharacterDefinition } from '../characters/catalog';
import React from 'react';
import { Canvas, Group, Path, Rect, Skia } from '@shopify/react-native-skia';
import { useDerivedValue, type SharedValue } from 'react-native-reanimated';
import { GAME } from '../engine/config';
import type { GameFrame } from '../engine/types';
import { level01 } from '../levels';
import { RobotPlayer } from './RobotPlayer';
import { PlayerTrail, SceneParticles } from './SceneEffects';
// Parse authored curves once. Frame updates contain IDs and transforms only;
// sending SVG strings each frame forces needless serialization and path parsing.
const PLATFORM_PATHS = level01.platforms.map(p => {
  const path = Skia.Path.MakeFromSVGString(p.d);
  if (!path) throw new Error(`Invalid platform path: ${p.id}`);
  return path;
});
const EMPTY_PATH = Skia.Path.Make();
const STAR =
  'M0 -11 L3.3 -4.5 L10.46 -3.4 L5.34 1.74 L6.47 8.9 L0 5.6 L-6.47 8.9 L-5.34 1.74 L-10.46 -3.4 L-3.3 -4.5 Z';
type SceneProps = { scene: SharedValue<GameFrame> };
function PlatformSlot({
  scene,
  index,
}: {
  scene: SharedValue<GameFrame>;
  index: number;
}) {
  const path = useDerivedValue(() => {
    const platform = scene.value.platforms[index];
    return platform ? PLATFORM_PATHS[platform.index] : EMPTY_PATH;
  });
  const transform = useDerivedValue(() => [
    {
      translateY:
        (scene.value.platforms[index]?.offset ?? 0) - scene.value.cameraY,
    },
  ]);
  const opacity = useDerivedValue(() => (scene.value.platforms[index] ? 1 : 0));
  const active = useDerivedValue(() =>
    scene.value.platforms[index]?.active ? 0.45 : 0.1,
  );
  return (
    <Group transform={transform} opacity={opacity}>
      <Path path={path} color="#FFFFFF" fillType="evenOdd" />
      <Path
        path={path}
        color="#AFBBC8"
        style="stroke"
        strokeWidth={1}
        opacity={active}
      />
    </Group>
  );
}
function StarSlot({
  scene,
  index,
}: {
  scene: SharedValue<GameFrame>;
  index: number;
}) {
  const transform = useDerivedValue(() => {
    const s = scene.value.stars[index];
    const scale = 1 + Math.sin(scene.value.time * 2.8 + index) * 0.04;
    return [
      { translateX: s?.x ?? -100 },
      { translateY: (s?.y ?? -100) - scene.value.cameraY },
      { scale },
    ];
  });
  return (
    <Group transform={transform}>
      <Path path={STAR} color="#303236" />
    </Group>
  );
}
export const GameCanvas = React.memo(function GameCanvasView({
  scene,
  width,
  height,
  character,
}: SceneProps & {
  width: number;
  height: number;
  character: CharacterDefinition;
}) {
  const scale = width / GAME.width;
  return (
    <Canvas style={{ width, height }}>
      <Rect x={0} y={0} width={width} height={height} color="#E2E6EB" />
      <Group transform={[{ scale }]}>
        <Rect x={0} y={0} width={2} height={height / scale} color="#B8C1CD" />
        <Rect x={388} y={0} width={2} height={height / scale} color="#B8C1CD" />
        {Array.from({ length: GAME.maxVisiblePlatforms }, (_, i) => (
          <PlatformSlot key={`p${i}`} scene={scene} index={i} />
        ))}
        {Array.from({ length: GAME.maxVisibleStars }, (_, i) => (
          <StarSlot key={`s${i}`} scene={scene} index={i} />
        ))}
        <PlayerTrail scene={scene} />
        <RobotPlayer scene={scene} character={character} />
        <SceneParticles scene={scene} />
      </Group>
    </Canvas>
  );
});
