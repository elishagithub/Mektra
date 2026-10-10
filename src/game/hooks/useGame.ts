import type { CharacterDefinition } from '../characters/catalog';
import { useCallback, useEffect, useRef, useState } from 'react';
import { AppState } from 'react-native';
import {
  useFrameCallback,
  useSharedValue,
  type FrameInfo,
} from 'react-native-reanimated';
import { runOnUI, scheduleOnRN } from 'react-native-worklets';
import { GameAudio } from '../../audio/GameAudio';
import { level01 } from '../levels';
import {
  createSimulation,
  simulationAdvance,
  simulationDrainEvents,
  simulationFrame,
  simulationJump,
  simulationPause,
  simulationResume,
  simulationSetViewport,
  simulationSetCharacter,
  simulationStart,
  simulationSummary,
} from '../engine/simulation';
import type { GameEvent, GameSummary } from '../engine/types';

type Command = 'start' | 'jump' | 'pause' | 'resume' | 'restart' | 'home';
export function useGame(
  viewportHeight: number,
  character: CharacterDefinition,
) {
  const [initial] = useState(() => createSimulation(level01));
  const simulation = useSharedValue(initial);
  const [initialFrame] = useState(() => simulationFrame(initial));
  const scene = useSharedValue(initialFrame);
  const active = useSharedValue(true);
  const frames = useSharedValue(0);
  const lastHUD = useSharedValue(0);
  const lastSummary = useSharedValue(simulationSummary(initial));
  const [summary, setSummary] = useState(() => simulationSummary(initial));
  const audio = useRef<GameAudio | null>(null);
  const [soundEnabled, setSoundEnabled] = useState(true);
  const report = useCallback((next: GameSummary) => setSummary(next), []);
  const playEvents = useCallback((events: GameEvent[]) => {
    audio.current?.playEvents(events);
  }, []);

  // Physics, camera and Skia scene stay together on the UI thread. React and audio
  // receive only occasional HUD changes/events, never per-frame scene snapshots.
  const tick = useCallback(
    (frame: FrameInfo) => {
      'worklet';
      if (!active.value) return;
      if (__DEV__) frames.value += 1;
      simulation.modify(state => {
        simulationAdvance(state, (frame.timeSincePreviousFrame ?? 0) / 1000);
        scene.value = {
          ...simulationFrame(state, true),
          time: frame.timestamp / 1000,
        };
        const events = simulationDrainEvents(state);
        if (events.length) scheduleOnRN(playEvents, events);
        if (
          frame.timestamp - lastHUD.value >= 100 ||
          state.phase !== lastSummary.value.phase
        ) {
          lastHUD.value = frame.timestamp;
          const next = simulationSummary(state),
            before = lastSummary.value;
          if (
            next.phase !== before.phase ||
            next.stars !== before.stars ||
            next.distance !== before.distance ||
            next.sections !== before.sections ||
            next.reason !== before.reason
          ) {
            lastSummary.value = next;
            scheduleOnRN(report, next);
          }
        }
        return state;
      });
    },
    [
      active,
      frames,
      simulation,
      scene,
      playEvents,
      lastHUD,
      lastSummary,
      report,
    ],
  );
  useFrameCallback(tick);

  const command = useCallback(
    (action: Command) => {
      runOnUI((name: Command) => {
        simulation.modify(state => {
          if (name === 'home' || name === 'restart') {
            Object.assign(state, createSimulation(state.level));
            simulationSetViewport(state, viewportHeight);
            simulationSetCharacter(
              state,
              character.legCount,
              character.legStyle,
            );
          }
          if (name === 'start' || name === 'restart') simulationStart(state);
          else if (name === 'jump') simulationJump(state);
          else if (name === 'pause') simulationPause(state);
          else if (name === 'resume') simulationResume(state);
          scene.value = simulationFrame(state, true);
          const next = simulationSummary(state);
          lastSummary.value = next;
          scheduleOnRN(report, next);
          return state;
        });
      })(action);
    },
    [simulation, scene, viewportHeight, report, lastSummary, character],
  );

  useEffect(() => {
    if (__DEV__) {
      const { installNativeChecks } =
        require('../dev/nativeChecks') as typeof import('../dev/nativeChecks');
      return installNativeChecks(frames);
    }
    return undefined;
  }, [frames]);
  useEffect(() => {
    const player = new GameAudio();
    audio.current = player;
    return () => {
      player.release();
      audio.current = null;
    };
  }, []);
  useEffect(() => {
    runOnUI((height: number) => {
      simulation.modify(state => {
        simulationSetViewport(state, height);
        scene.value = simulationFrame(state);
        return state;
      });
    })(viewportHeight);
  }, [viewportHeight, simulation, scene]);
  useEffect(() => {
    runOnUI((count: number, style: CharacterDefinition['legStyle']) => {
      simulation.modify(state => {
        simulationSetCharacter(state, count, style);
        scene.value = simulationFrame(state);
        return state;
      });
    })(character.legCount, character.legStyle);
  }, [character, simulation, scene]);
  useEffect(() => {
    const listener = AppState.addEventListener('change', state => {
      active.value = state === 'active';
      if (state !== 'active') {
        command('pause');
        audio.current?.stop();
      }
    });
    return () => listener.remove();
  }, [active, command]);

  const start = () => {
    command('start');
    audio.current?.play('hello');
  };
  const restart = () => {
    audio.current?.stop();
    command('restart');
    audio.current?.play('hello');
  };
  const home = () => {
    audio.current?.stop();
    command('home');
  };
  const pause = () => {
    command('pause');
    audio.current?.stop();
  };
  const toggleSound = () => {
    const enabled = !soundEnabled;
    audio.current?.setEnabled(enabled);
    setSoundEnabled(enabled);
  };
  return {
    scene,
    summary,
    start,
    jump: () => command('jump'),
    pause,
    resume: () => command('resume'),
    restart,
    home,
    soundEnabled,
    toggleSound,
  };
}
