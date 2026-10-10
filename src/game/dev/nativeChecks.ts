import type { SharedValue } from 'react-native-reanimated';
import { runOnUI, scheduleOnRN } from 'react-native-worklets';
import { level01 } from '../levels';
import plans from '../../../assets/levels/jump-plan.json';
import { GAME } from '../engine/config';
import { orbitPose } from '../engine/orbit';
import {
  createSimulation,
  simulationAdvance,
  simulationJump,
  simulationPause,
  simulationResume,
  simulationSetViewport,
  simulationStart,
} from '../engine/simulation';

type Report = {
  jumps: number;
  failures: string[];
  uiFramesDuringJSStall: number;
};
type DebugGlobal = typeof globalThis & {
  __mektraNativeChecks?: () => Promise<Report>;
};
/** Opt-in development probe; independent simulations never alter the player's run. */
export function installNativeChecks(frames: SharedValue<number>) {
  const debug = globalThis as DebugGlobal;
  const readFrames = () =>
    new Promise<number>(resolve => {
      runOnUI(() => {
        scheduleOnRN(resolve, frames.value);
      })();
    });
  debug.__mektraNativeChecks = async () => {
    const physics = await new Promise<{ jumps: number; failures: string[] }>(
      resolve => {
        runOnUI(() => {
          const failures: string[] = [];
          for (let i = 0; i < plans.length; i++) {
            const state = createSimulation(level01);
            const pose = orbitPose(state.orbits[i], plans[i].distance);
            Object.assign(state.player, pose, {
              platformKey: `0:p:${i}`,
              distance: plans[i].distance,
              mode: 'orbit',
            });
            simulationSetViewport(state, 750);
            simulationStart(state);
            simulationJump(state);
            for (
              let n = 0;
              n < 240 &&
              state.player.mode === 'flight' &&
              state.phase === 'playing';
              n++
            )
              simulationAdvance(state, GAME.fixedStep);
            const target = i === plans.length - 1 ? '1:p:0' : `0:p:${i + 1}`;
            if (
              state.phase !== 'playing' ||
              state.player.platformKey !== target ||
              state.score === 0
            )
              failures.push(`jump ${i + 1}`);
            simulationPause(state);
            const x = state.player.x,
              y = state.player.y;
            simulationAdvance(state, 1);
            if (state.player.x !== x || state.player.y !== y)
              failures.push(`pause ${i + 1}`);
            simulationResume(state);
          }
          scheduleOnRN(resolve, { jumps: plans.length, failures });
        })();
      },
    );
    const before = await readFrames();
    // Deliberately block only JS. UI simulation/render callbacks must keep ticking.
    const started = Date.now();
    while (Date.now() - started < 300) {
      /* diagnostic stall */
    }
    const after = await readFrames();
    return { ...physics, uiFramesDuringJSStall: after - before };
  };
  return () => {
    delete debug.__mektraNativeChecks;
  };
}
