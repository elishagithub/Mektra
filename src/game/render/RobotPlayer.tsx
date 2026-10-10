import React, { useMemo } from 'react';
import {
  Circle,
  Group,
  Path,
  RoundedRect,
  Skia,
} from '@shopify/react-native-skia';
import { useDerivedValue, type SharedValue } from 'react-native-reanimated';
import type { GameFrame } from '../engine/types';
import { GAME } from '../engine/config';
import { robotLegPath, ROBOT_SHELLS } from './robotGeometry';
import type { CharacterDefinition } from '../characters/catalog';

// Same vector proportions as the editable Figma Mini Robo expression sheet.
const SHELL_HIGHLIGHT = Skia.Path.MakeFromSVGString('M-8 -10Q0 -14 8 -10')!;
const HAPPY_EYES = Skia.Path.MakeFromSVGString(
  'M-8 0Q-5.5 -5 -3 0M3 0Q5.5 -5 8 0',
)!;
const MOUTH = Skia.Path.MakeFromSVGString('M-2 5h4')!;

export function RobotPlayer({
  scene,
  character,
  scale = GAME.playerScale,
  showAim = true,
}: {
  scene: SharedValue<GameFrame>;
  character: CharacterDefinition;
  scale?: number;
  showAim?: boolean;
}) {
  const legs = useMemo(
    () =>
      Skia.Path.MakeFromSVGString(
        robotLegPath(character.legCount, character.legStyle),
      )!,
    [character.legCount, character.legStyle],
  );
  const shell = useMemo(
    () => Skia.Path.MakeFromSVGString(ROBOT_SHELLS[character.shape])!,
    [character.shape],
  );
  const happyPath = useMemo(
    () =>
      character.eyeStyle === 'mono'
        ? Skia.Path.MakeFromSVGString('M-4 0Q0 -5 4 0')!
        : HAPPY_EYES,
    [character.eyeStyle],
  );
  const color = character.color,
    led = character.eyeColor;
  const mono = character.eyeStyle === 'mono';
  const round = character.eyeStyle === 'round';
  const eyeWidth = mono
    ? 8
    : round
    ? 6
    : character.eyeStyle === 'visor'
    ? 7
    : 5;
  const opacity = useDerivedValue(() => (scene.value.player.visible ? 1 : 0));
  const transform = useDerivedValue(() => [
    { translateX: scene.value.player.x },
    { translateY: scene.value.player.y - scene.value.cameraY },
    { scale },
  ]);
  // Only the ring rotates. The shell and face remain upright in screen space.
  const legRotation = useDerivedValue(() => [
    { rotate: scene.value.player.gearAngle },
  ]);
  const aimRotation = useDerivedValue(() => [
    {
      rotate:
        Math.atan2(scene.value.player.ny, scene.value.player.nx) + Math.PI / 2,
    },
  ]);
  const eyeHeight = useDerivedValue(() => {
    const t = scene.value.time % 4.1;
    const blink = t > 3.92 ? Math.max(0.12, Math.abs(t - 4.01) / 0.09) : 1;
    return (
      (scene.value.player.expression === 'focused'
        ? 4
        : scene.value.player.expression === 'surprised'
        ? 9
        : character.eyeStyle === 'visor'
        ? 3
        : 7) * blink
    );
  });
  const eyeY = useDerivedValue(() => -0.5 - eyeHeight.value / 2);
  const regularEyes = useDerivedValue(() =>
    scene.value.player.expression === 'happy' ? 0 : 1,
  );
  const happyEyes = useDerivedValue(() =>
    scene.value.player.expression === 'happy' ? 1 : 0,
  );
  const mouthOpacity = useDerivedValue(() =>
    scene.value.player.expression === 'surprised' ? 0 : 1,
  );
  const surpriseOpacity = useDerivedValue(() =>
    scene.value.player.expression === 'surprised' ? 1 : 0,
  );
  const aim = useDerivedValue(() =>
    showAim && !scene.value.player.flying ? 0.35 : 0,
  );
  return (
    <Group transform={transform} opacity={opacity}>
      <Group transform={legRotation}>
        <Path path={legs} color={color} />
        <Path
          path={legs}
          color={character.shellStroke}
          style="stroke"
          strokeWidth={0.5}
          strokeJoin="round"
        />
      </Group>
      <Path path={shell} color={color} />
      <Path
        path={shell}
        color={character.shellStroke}
        style="stroke"
        strokeWidth={1}
      />
      {character.shape !== 'diamond' && (
        <Path
          path={SHELL_HIGHLIGHT}
          color={character.id === 'mektra' ? '#DAE8F8' : 'rgba(255,255,255,.7)'}
          style="stroke"
          strokeWidth={0.8}
          strokeCap="round"
        />
      )}
      <Group transform={[{ scale: character.shape === 'diamond' ? 0.8 : 1 }]}>
        <RoundedRect
          x={-10}
          y={-9}
          width={20}
          height={18}
          r={5}
          color={character.faceColor}
        />
        <RoundedRect
          x={-10}
          y={-9}
          width={20}
          height={18}
          r={5}
          color={character.shellStroke}
          style="stroke"
          strokeWidth={0.7}
        />
        <Group opacity={regularEyes}>
          <RoundedRect
            x={mono ? -4 : round ? -8.5 : -8}
            y={eyeY}
            width={eyeWidth}
            height={eyeHeight}
            r={round || mono ? 3 : 2}
            color={led}
          />
          {!mono && (
            <RoundedRect
              x={round ? 2.5 : 3}
              y={eyeY}
              width={eyeWidth}
              height={eyeHeight}
              r={round || mono ? 3 : 2}
              color={led}
            />
          )}
        </Group>
        <Path
          path={happyPath}
          opacity={happyEyes}
          color={led}
          style="stroke"
          strokeWidth={2}
          strokeCap="round"
        />
        <Path
          path={MOUTH}
          opacity={mouthOpacity}
          color={led}
          style="stroke"
          strokeWidth={1}
          strokeCap="round"
        />
        <Circle cx={0} cy={5} r={1.5} color={led} opacity={surpriseOpacity} />
        <Circle cx={0} cy={11} r={1} color={led} />
      </Group>
      <Group opacity={aim} transform={aimRotation}>
        <Circle cx={0} cy={-28} r={2} color="#303236" />
        <Circle cx={0} cy={-40} r={1.5} color="#303236" />
        <Circle cx={0} cy={-51} r={1} color="#303236" />
      </Group>
    </Group>
  );
}
