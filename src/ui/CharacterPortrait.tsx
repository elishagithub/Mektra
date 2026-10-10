import { StyleSheet } from 'react-native';
import React, { useEffect, useState } from 'react';
import { Canvas } from '@shopify/react-native-skia';
import { useSharedValue } from 'react-native-reanimated';
import type { CharacterDefinition } from '../game/characters/catalog';
import type { GameFrame } from '../game/engine/types';
import { RobotPlayer } from '../game/render/RobotPlayer';

/** The shop draws the actual in-game artwork, never a separate thumbnail. */
export const CharacterPortrait = React.memo(function CharacterPortraitView({
  character,
  selected,
}: {
  character: CharacterDefinition;
  selected: boolean;
}) {
  const [initial] = useState<GameFrame>(() => ({
    player: {
      x: 66,
      y: 60,
      nx: 0,
      ny: -1,
      visible: true,
      flying: false,
      expression: selected ? 'happy' : 'idle',
      gearAngle: 0.12,
    },
    cameraY: 0,
    time: 0,
    platforms: [],
    stars: [],
    particles: [],
    bursts: [],
    trail: [],
  }));
  const scene = useSharedValue(initial);
  useEffect(() => {
    scene.value = {
      ...initial,
      player: { ...initial.player, expression: selected ? 'happy' : 'idle' },
    };
  }, [selected, initial, scene]);
  return (
    <Canvas style={styles.canvas} accessible={false}>
      <RobotPlayer
        scene={scene}
        character={character}
        scale={2.65}
        showAim={false}
      />
    </Canvas>
  );
});

const styles = StyleSheet.create({ canvas: { width: 132, height: 120 } });
